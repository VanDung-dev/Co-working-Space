using System.Security.Claims;
using Co_working_Space.Areas.Admin.Controllers;
using Co_working_Space.Models;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Co_working_Space.Nunit.Admin;

[TestFixture]
public class AdminBookingControllerTests
{
    private Mock<IApprovalService> _approvalService = null!;
    private BookingController _controller = null!;
    private const string StaffId = "STF-0001";

    [TearDown]
    public void TearDown() => _controller?.Dispose();

    [SetUp]
    public void SetUp()
    {
        _approvalService = new Mock<IApprovalService>();
        _controller = new BookingController(_approvalService.Object);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, StaffId) };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) }
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }

    [Test]
    public async Task Pending_ReturnsViewWithBookings()
    {
        var bookings = new List<Booking>
        {
            new() { Id = "BKG-001", Title = "Test", RoomId = "RM-M-001", UserId = "USR-0001" }
        };
        _approvalService.Setup(x => x.GetPendingAsync()).ReturnsAsync(bookings);

        var result = await _controller.Pending();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<Booking>;
        Assert.That(model, Is.EqualTo(bookings));
    }

    [Test]
    public async Task Approve_Success_RedirectsWithSuccess()
    {
        _approvalService.Setup(x => x.ApproveAsync("BKG-001", StaffId))
            .ReturnsAsync((true, null));

        var result = await _controller.Approve("BKG-001");

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Pending"));
        Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Đã duyệt đơn đặt phòng."));
    }

    [Test]
    public async Task Approve_Failure_RedirectsWithError()
    {
        _approvalService.Setup(x => x.ApproveAsync("BKG-001", StaffId))
            .ReturnsAsync((false, "Số dư không đủ."));

        var result = await _controller.Approve("BKG-001");

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Pending"));
        Assert.That(_controller.TempData["ErrorMessage"], Is.EqualTo("Số dư không đủ."));
    }

    [Test]
    public async Task Reject_Success_RedirectsWithSuccess()
    {
        _approvalService.Setup(x => x.RejectAsync("BKG-001", StaffId, "Phòng bận"))
            .ReturnsAsync(true);

        var result = await _controller.Reject("BKG-001", "Phòng bận");

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Pending"));
        Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo("Đã từ chối đơn đặt phòng."));
    }

    [Test]
    public async Task Reject_Failure_ReturnsNotFound()
    {
        _approvalService.Setup(x => x.RejectAsync("BKG-NONEXIST", StaffId, "Lý do"))
            .ReturnsAsync(false);

        var result = await _controller.Reject("BKG-NONEXIST", "Lý do");

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
