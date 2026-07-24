using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Co_working_Space.Services;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Nunit.Services;

[TestFixture]
public class BookingServiceOverlapTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var db = new ApplicationDbContext(options);

        db.Rooms.Add(new Room
        {
            Id = "RM-M-001", Name = "Phòng A", Capacity = 6,
            PricePerHour = 100_000, IsActive = true
        });

        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-001", RoomId = "RM-M-001", UserId = "USR-0001",
            Title = "Existing",
            StartTime = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Approved
        });

        db.SaveChanges();
        return db;
    }

    [Test]
    public async Task HasOverlapAsync_WhenOverlapExists_ReturnsTrue()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 10, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasOverlapAsync_WhenNoOverlap_ReturnsFalse()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_ExcludesCancelledBookings()
    {
        using var db = CreateDb();
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-002", RoomId = "RM-M-001", UserId = "USR-0002",
            StartTime = new DateTime(2026, 7, 20, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 20, 15, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Cancelled
        });
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 14, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 15, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_ExcludesRejectedBookings()
    {
        using var db = CreateDb();
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-003", RoomId = "RM-M-001", UserId = "USR-0002",
            StartTime = new DateTime(2026, 7, 20, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 20, 15, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Rejected
        });
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 14, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 15, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_DetectsPendingBookings()
    {
        using var db = CreateDb();
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-004", RoomId = "RM-M-001", UserId = "USR-0002",
            StartTime = new DateTime(2026, 7, 20, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 20, 15, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Pending
        });
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 14, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 15, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasOverlapAsync_ExcludesDifferentRoom()
    {
        using var db = CreateDb();
        db.Rooms.Add(new Room
        {
            Id = "RM-S-001", Name = "Phòng B", Capacity = 4,
            PricePerHour = 50_000, IsActive = true
        });
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-S-001",
            new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 10, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_WhenBoundaryTouches_ReturnsFalse()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_WhenUpdating_ExcludesOwnBooking()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            currentBookingId: "BKG-20260720-001");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_WhenNewBookingFullyInside_ReturnsTrue()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 9, 15, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 9, 45, 0, DateTimeKind.Utc));

        Assert.That(result, Is.True);
    }
}
