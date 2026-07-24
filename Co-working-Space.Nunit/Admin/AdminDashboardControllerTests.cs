using Co_working_Space.Areas.Admin.Controllers;
using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Nunit.Admin;

[TestFixture]
public class AdminDashboardControllerTests
{
    private ApplicationDbContext _db = null!;
    private DashboardController _controller = null!;

    [TearDown]
    public void TearDown()
    {
        _db?.Dispose();
        _controller?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);
        _controller = new DashboardController(_db);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _db.Rooms.AddRange(
            new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 2" },
            new Room { Id = "RM-M-002", Name = "Phòng B", Capacity = 8, PricePerHour = 150_000, Location = "Tầng 2" }
        );
        _db.SaveChanges();
    }



    [Test]
    public async Task Index_ReturnsView()
    {
        var result = await _controller.Index();
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_CountsMonthlyBookings()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        _db.Bookings.AddRange(
            new Booking { Id = "BKG-001", RoomId = "RM-M-001", UserId = "USR-0001", Title = "T1", StartTime = now, EndTime = now.AddHours(1), CreatedAt = monthStart.AddDays(1), Status = BookingStatus.Approved },
            new Booking { Id = "BKG-002", RoomId = "RM-M-001", UserId = "USR-0001", Title = "T2", StartTime = now, EndTime = now.AddHours(1), CreatedAt = monthStart.AddDays(2), Status = BookingStatus.Pending },
            new Booking { Id = "BKG-003", RoomId = "RM-M-001", UserId = "USR-0001", Title = "T3", StartTime = now, EndTime = now.AddHours(1), CreatedAt = monthStart.AddMonths(-1), Status = BookingStatus.Approved }
        );
        await _db.SaveChangesAsync();

        await _controller.Index();

        Assert.That(_controller.ViewBag.MonthlyBookings, Is.EqualTo(2));
    }

    [Test]
    public async Task Index_ShowsMostUsedRooms()
    {
        var now = DateTime.UtcNow;
        _db.Bookings.AddRange(
            new Booking { Id = "BKG-001", RoomId = "RM-M-002", UserId = "USR-0001", Title = "T1", StartTime = now, EndTime = now.AddHours(1), Status = BookingStatus.Approved },
            new Booking { Id = "BKG-002", RoomId = "RM-M-002", UserId = "USR-0001", Title = "T2", StartTime = now, EndTime = now.AddHours(1), Status = BookingStatus.Approved },
            new Booking { Id = "BKG-003", RoomId = "RM-M-001", UserId = "USR-0001", Title = "T3", StartTime = now, EndTime = now.AddHours(1), Status = BookingStatus.Approved }
        );
        await _db.SaveChangesAsync();

        await _controller.Index();

        var mostUsed = _controller.ViewBag.MostUsedRooms;
        Assert.That(mostUsed, Is.Not.Null);
    }
}
