using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Co_working_Space.Services;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Nunit.Services;

[TestFixture]
public class ApprovalServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var db = new ApplicationDbContext(options);

        db.Users.Add(new Microsoft.AspNetCore.Identity.IdentityUser { Id = "USR-0001", UserName = "user1@example.com", Email = "user1@example.com" });
        db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, IsActive = true });
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-001", RoomId = "RM-M-001", UserId = "USR-0001",
            Title = "Test Meeting",
            StartTime = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            TotalPrice = 100_000,
            Status = BookingStatus.Pending
        });
        db.Wallets.Add(new Wallet { UserId = "USR-0001", Balance = 500_000 });
        db.SaveChanges();
        return db;
    }

    [Test]
    public async Task GetPendingAsync_ReturnsOnlyPending()
    {
        using var db = CreateDb();
        db.Bookings.Add(new Booking { Id = "BKG-20260721-001", RoomId = "RM-M-001", UserId = "USR-0001", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Status = BookingStatus.Approved });
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);
        var pending = await service.GetPendingAsync();

        Assert.That(pending, Has.Count.EqualTo(1));
        Assert.That(pending[0].Id, Is.EqualTo("BKG-20260720-001"));
    }

    [Test]
    public async Task GetPendingAsync_OrdersByStartTime()
    {
        using var db = CreateDb();
        db.Bookings.Add(new Booking { Id = "BKG-20260722-001", RoomId = "RM-M-001", UserId = "USR-0001", StartTime = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc), EndTime = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc), Status = BookingStatus.Pending });
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);
        var pending = await service.GetPendingAsync();

        Assert.That(pending[0].StartTime, Is.EqualTo(new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc)));
    }

    [Test]
    public async Task ApproveAsync_Valid_DeductsWalletAndCreatesApproval()
    {
        using var db = CreateDb();
        var service = new ApprovalService(db);

        var (success, error) = await service.ApproveAsync("BKG-20260720-001", "STF-0001");

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(error, Is.Null);
        });

        var booking = await db.Bookings.FindAsync("BKG-20260720-001");
        Assert.That(booking!.Status, Is.EqualTo(BookingStatus.Approved));
        Assert.That(booking.PaymentStatus, Is.EqualTo(PaymentStatus.Paid));
        Assert.That(booking.PaidAt, Is.Not.Null);

        var wallet = await db.Wallets.FindAsync("USR-0001");
        Assert.That(wallet!.Balance, Is.EqualTo(400_000));

        var approval = await db.BookingApprovals.FirstOrDefaultAsync(a => a.BookingId == "BKG-20260720-001");
        Assert.That(approval, Is.Not.Null);
        Assert.That(approval!.ApproverId, Is.EqualTo("STF-0001"));
    }

    [Test]
    public async Task ApproveAsync_NonPendingBooking_ReturnsError()
    {
        using var db = CreateDb();
        var booking = await db.Bookings.FindAsync("BKG-20260720-001");
        booking!.Status = BookingStatus.Approved;
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        var (success, error) = await service.ApproveAsync("BKG-20260720-001", "STF-0001");

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.False);
            Assert.That(error, Does.Contain("không ở trạng thái chờ duyệt"));
        });
    }

    [Test]
    public async Task ApproveAsync_InsufficientBalance_ReturnsError()
    {
        using var db = CreateDb();
        var wallet = await db.Wallets.FindAsync("USR-0001");
        wallet!.Balance = 50_000;
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        var (success, error) = await service.ApproveAsync("BKG-20260720-001", "STF-0001");

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.False);
            Assert.That(error, Does.Contain("Số dư không đủ"));
        });
    }

    [Test]
    public async Task ApproveAsync_NullWallet_ReturnsError()
    {
        using var db = CreateDb();
        var wallet = await db.Wallets.FindAsync("USR-0001");
        db.Wallets.Remove(wallet!);
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        var (success, error) = await service.ApproveAsync("BKG-20260720-001", "STF-0001");

        Assert.That(success, Is.False);
    }

    [Test]
    public async Task RejectAsync_Pending_ChangesStatus()
    {
        using var db = CreateDb();
        var service = new ApprovalService(db);

        var result = await service.RejectAsync("BKG-20260720-001", "STF-0001", "Phòng bận");

        Assert.That(result, Is.True);

        var booking = await db.Bookings.FindAsync("BKG-20260720-001");
        Assert.That(booking!.Status, Is.EqualTo(BookingStatus.Rejected));

        var approval = await db.BookingApprovals.FirstOrDefaultAsync(a => a.BookingId == "BKG-20260720-001");
        Assert.That(approval, Is.Not.Null);
        Assert.That(approval!.Reason, Is.EqualTo("Phòng bận"));
    }

    [Test]
    public async Task RejectAsync_WhenPaid_RefundsWallet()
    {
        using var db = CreateDb();
        var booking = await db.Bookings.FindAsync("BKG-20260720-001");
        booking!.PaymentStatus = PaymentStatus.Paid;
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        await service.RejectAsync("BKG-20260720-001", "STF-0001", "Trùng lịch");

        var wallet = await db.Wallets.FindAsync("USR-0001");
        Assert.That(wallet!.Balance, Is.EqualTo(600_000));
        Assert.That(booking.PaymentStatus, Is.EqualTo(PaymentStatus.Refunded));
    }

    [Test]
    public async Task RejectAsync_NonPendingBooking_ReturnsFalse()
    {
        using var db = CreateDb();
        var booking = await db.Bookings.FindAsync("BKG-20260720-001");
        booking!.Status = BookingStatus.Approved;
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        var result = await service.RejectAsync("BKG-20260720-001", "STF-0001", "Lý do");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RejectAsync_NonexistentBooking_ReturnsFalse()
    {
        using var db = CreateDb();
        var service = new ApprovalService(db);

        var result = await service.RejectAsync("BKG-NONEXIST", "STF-0001", "Lý do");

        Assert.That(result, Is.False);
    }
}
