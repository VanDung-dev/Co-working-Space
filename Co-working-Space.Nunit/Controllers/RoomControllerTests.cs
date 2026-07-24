using Co_working_Space.Controllers;
using Co_working_Space.Models;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Co_working_Space.Nunit.Controllers;

[TestFixture]
public class RoomControllerTests
{
    private Mock<IRoomService> _roomService = null!;
    private RoomController _controller = null!;

    [TearDown]
    public void TearDown() => _controller?.Dispose();

    [SetUp]
    public void SetUp()
    {
        _roomService = new Mock<IRoomService>();
        _controller = new RoomController(_roomService.Object);
    }

    [Test]
    public async Task Index_NoFilters_CallsSearchWithNulls()
    {
        _roomService.Setup(x => x.SearchAsync(null, null, null))
            .ReturnsAsync([]);

        var result = await _controller.Index(null, null, null);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        _roomService.Verify(x => x.SearchAsync(null, null, null), Times.Once);
    }

    [Test]
    public async Task Index_WithFilters_PassesToService()
    {
        _roomService.Setup(x => x.SearchAsync(5, "Tầng 2", It.IsAny<List<string>?>()))
            .ReturnsAsync([]);

        var result = await _controller.Index(5, "Tầng 2", "Máy chiếu,Tivi");

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        _roomService.Verify(x => x.SearchAsync(5, "Tầng 2",
            It.Is<List<string>>(l => l!.Count == 2 && l[0] == "Máy chiếu")), Times.Once);
    }

    [Test]
    public async Task Index_PassesRoomsToView()
    {
        var rooms = new List<Room>
        {
            new() { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 2", IsActive = true }
        };
        _roomService.Setup(x => x.SearchAsync(null, null, null)).ReturnsAsync(rooms);

        var result = await _controller.Index(null, null, null);

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<Room>;
        Assert.That(model, Is.EqualTo(rooms));
    }
}
