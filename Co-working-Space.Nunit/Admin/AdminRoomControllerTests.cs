using Co_working_Space.Areas.Admin.Controllers;
using Co_working_Space.Data;
using Co_working_Space.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Co_working_Space.Nunit.Admin;

[TestFixture]
public class AdminRoomControllerTests
{
    private ApplicationDbContext _db = null!;
    private RoomController _controller = null!;

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
        _controller = new RoomController(_db);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }



    [Test]
    public async Task Index_ReturnsAllRooms()
    {
        _db.Rooms.AddRange(
            new Room { Id = "RM-S-001", Name = "B. Phòng Nhỏ", Capacity = 4, PricePerHour = 50_000, Location = "Tầng 1" },
            new Room { Id = "RM-M-001", Name = "A. Phòng Vừa", Capacity = 8, PricePerHour = 100_000, Location = "Tầng 2" }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.Index();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<Room>;
        Assert.That(model, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ToggleStatus_TogglesIsActive()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 2", IsActive = true });
        await _db.SaveChangesAsync();

        await _controller.ToggleStatus("RM-M-001");

        var room = await _db.Rooms.FindAsync("RM-M-001");
        Assert.That(room!.IsActive, Is.False);

        await _controller.ToggleStatus("RM-M-001");
        Assert.That(room!.IsActive, Is.True);
    }

    [Test]
    public async Task ToggleStatus_Nonexistent_ReturnsNotFound()
    {
        var result = await _controller.ToggleStatus("RM-NONEXIST");
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_ValidRoom_CreatesWithAutoId()
    {
        var room = new Room { Name = "Phòng Test", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 3" };

        var result = await _controller.Create(room, null);

        var redirect = result as RedirectToActionResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect!.ActionName, Is.EqualTo("Index"));

        var saved = await _db.Rooms.FirstOrDefaultAsync(r => r.Name == "Phòng Test");
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Id, Does.StartWith("RM-M-"));
    }

    [Test]
    public async Task Create_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Name", "Required");

        var result = await _controller.Create(new Room { Capacity = 6, PricePerHour = 100_000 }, null);

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Create_SmallRoom_GetsSmallPrefix()
    {
        var room = new Room { Name = "Nhỏ", Capacity = 2, PricePerHour = 50_000, Location = "Tầng 1" };
        await _controller.Create(room, null);
        var saved = await _db.Rooms.FirstAsync(r => r.Name == "Nhỏ");
        Assert.That(saved.Id, Does.StartWith("RM-S-"));
    }

    [Test]
    public async Task Create_LargeRoom_GetsLargePrefix()
    {
        var room = new Room { Name = "Lớn", Capacity = 12, PricePerHour = 200_000, Location = "Tầng 2" };
        await _controller.Create(room, null);
        var saved = await _db.Rooms.FirstAsync(r => r.Name == "Lớn");
        Assert.That(saved.Id, Does.StartWith("RM-L-"));
    }

    [Test]
    public async Task Edit_Get_Existing_ReturnsView()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 2" });
        await _db.SaveChangesAsync();

        var result = await _controller.Edit("RM-M-001");

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        var model = viewResult!.Model as Room;
        Assert.That(model!.Name, Is.EqualTo("Phòng A"));
    }

    [Test]
    public async Task Edit_Get_Nonexistent_ReturnsNotFound()
    {
        var result = await _controller.Edit("RM-NONEXIST");
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task ManageEquipment_Get_ReturnsViewWithData()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 2" });
        _db.Equipment.Add(new Equipment { Id = "EQ-PROJ-001", Name = "Máy chiếu" });
        await _db.SaveChangesAsync();

        var result = await _controller.ManageEquipment("RM-M-001");

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
    }

    [Test]
    public async Task ManageEquipment_Nonexistent_ReturnsNotFound()
    {
        var result = await _controller.ManageEquipment("RM-NONEXIST");
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
