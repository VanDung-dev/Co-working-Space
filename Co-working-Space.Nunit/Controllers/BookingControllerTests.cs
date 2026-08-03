using System.Security.Claims;
using Co_working_Space.Controllers;
using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Co_working_Space.Models.ViewModels;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Co_working_Space.Nunit.Controllers;

[TestFixture]
public class BookingControllerTests
{
    private Mock<IBookingService> _bookingService = null!;
    private ApplicationDbContext _db = null!;
    private BookingController _controller = null!;
    private const string UserId = "USR-0001";

    [TearDown]
    public void TearDown()
    {
        _db?.Dispose();
        _controller?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _bookingService = new Mock<IBookingService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);

        _controller = new BookingController(_bookingService.Object, _db);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, UserId) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }



    [Test]
    public async Task Create_Get_SetsViewBagRoomId()
    {
        var result = await _controller.Create("RM-M-001");

        Assert.That(result, Is.InstanceOf<ViewResult>());
        Assert.That(_controller.ViewBag.RoomId, Is.EqualTo("RM-M-001"));
    }

    [Test]
    public async Task Create_Post_Success_RedirectsToMyBookings()
    {
        _bookingService.Setup(x => x.CreateBookingAsync(It.IsAny<CreateBookingViewModel>(), UserId))
            .ReturnsAsync("BKG-20260720-001");

        var result = await _controller.Create(new CreateBookingViewModel
        {
            RoomId = "RM-M-001", Title = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
        });

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("MyBookings"));
    }

    [Test]
    public async Task Create_Post_Failure_ReturnsViewWithError()
    {
        _bookingService.Setup(x => x.CreateBookingAsync(It.IsAny<CreateBookingViewModel>(), UserId))
            .ReturnsAsync((string?)null);

        var result = await _controller.Create(new CreateBookingViewModel
        {
            RoomId = "RM-M-001", Title = "Test",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
        });

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState[string.Empty]?.Errors[0].ErrorMessage,
            Is.EqualTo("Thời gian đặt phòng không hợp lệ hoặc đã bị trùng."));
    }

    [Test]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Title", "Required");

        var result = await _controller.Create(new CreateBookingViewModel());

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task MyBookings_ReturnsUserBookings()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, IsActive = true });
        _db.Bookings.AddRange(
            new Booking { Id = "BKG-001", UserId = UserId, RoomId = "RM-M-001", Title = "Mine", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Status = BookingStatus.Pending, CreatedAt = DateTime.UtcNow },
            new Booking { Id = "BKG-002", UserId = "USR-OTHER", RoomId = "RM-M-001", Title = "Not Mine", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Status = BookingStatus.Pending, CreatedAt = DateTime.UtcNow }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.MyBookings();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<Booking>;
        Assert.That(model, Has.Count.EqualTo(1));
        Assert.That(model![0].Id, Is.EqualTo("BKG-001"));
    }

    [Test]
    public async Task Cancel_OwnPendingBooking_Succeeds()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, IsActive = true });
        _db.Bookings.Add(new Booking
        {
            Id = "BKG-001", UserId = UserId, RoomId = "RM-M-001", Title = "Test",
            StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1),
            Status = BookingStatus.Pending
        });
        await _db.SaveChangesAsync();

        var result = await _controller.Cancel("BKG-001");

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var booking = await _db.Bookings.FindAsync("BKG-001");
        Assert.That(booking!.Status, Is.EqualTo(BookingStatus.Cancelled));
    }

    [Test]
    public async Task Cancel_NotOwnBooking_ReturnsNotFound()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, IsActive = true });
        _db.Bookings.Add(new Booking
        {
            Id = "BKG-001", UserId = "USR-OTHER", RoomId = "RM-M-001", Title = "Test",
            StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1),
            Status = BookingStatus.Pending
        });
        await _db.SaveChangesAsync();

        var result = await _controller.Cancel("BKG-001");

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Cancel_ApprovedBooking_ShowsError()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, IsActive = true });
        _db.Bookings.Add(new Booking
        {
            Id = "BKG-001", UserId = UserId, RoomId = "RM-M-001", Title = "Test",
            StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1),
            Status = BookingStatus.Approved
        });
        await _db.SaveChangesAsync();

        var result = await _controller.Cancel("BKG-001");

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(_controller.TempData["ErrorMessage"], Is.EqualTo("Chỉ có thể hủy đơn ở trạng thái Chờ duyệt."));
        var booking = await _db.Bookings.FindAsync("BKG-001");
        Assert.That(booking!.Status, Is.EqualTo(BookingStatus.Approved));
    }
}
