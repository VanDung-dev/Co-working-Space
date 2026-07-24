using Co_working_Space.Areas.Admin.Controllers;
using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Co_working_Space.Nunit.Admin;

[TestFixture]
public class AdminEquipmentControllerTests
{
    private ApplicationDbContext _db = null!;
    private EquipmentController _controller = null!;

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
        _controller = new EquipmentController(_db);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());
    }



    [Test]
    public async Task Index_ReturnsAllEquipment()
    {
        _db.Equipment.AddRange(
            new Equipment { Id = "EQ-PROJ-001", Name = "Máy chiếu" },
            new Equipment { Id = "EQ-TV-001", Name = "Tivi" }
        );
        await _db.SaveChangesAsync();

        var result = await _controller.Index();

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as List<Equipment>;
        Assert.That(model, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Create_Valid_AddsEquipment()
    {
        var result = await _controller.Create("Máy chiếu Epson", "Máy chiếu full HD");

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var equipment = await _db.Equipment.FirstOrDefaultAsync(e => e.Name == "Máy chiếu Epson");
        Assert.That(equipment, Is.Not.Null);
        Assert.That(equipment!.Id, Does.StartWith("EQ-PROJ-"));
        Assert.That(equipment.Status, Is.EqualTo(EquipmentStatus.Available));
    }

    [Test]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        var result = await _controller.Create("", null);
        Assert.That(result, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task Create_DetectsPrefixByName()
    {
        await _controller.Create("Tivi Sony", null);
        await _controller.Create("Micro không dây", null);
        await _controller.Create("Bảng trắng", null);
        await _controller.Create("Loa Bluetooth", null);
        await _controller.Create("Camera Logitech", null);

        var all = await _db.Equipment.ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(all.Any(e => e.Id.StartsWith("EQ-TV-")), Is.True);
            Assert.That(all.Any(e => e.Id.StartsWith("EQ-MIC-")), Is.True);
            Assert.That(all.Any(e => e.Id.StartsWith("EQ-WB-")), Is.True);
            Assert.That(all.Any(e => e.Id.StartsWith("EQ-SPK-")), Is.True);
            Assert.That(all.Any(e => e.Id.StartsWith("EQ-CAM-")), Is.True);
        });
    }

    [Test]
    public async Task Create_FallbackPrefix()
    {
        await _controller.Create("Đầu ghi HDMI", null);
        var equipment = await _db.Equipment.FirstAsync();
        Assert.That(equipment.Id, Does.StartWith("EQ-CAP-"));
    }

    [Test]
    public async Task Delete_Existing_Removes()
    {
        _db.Equipment.Add(new Equipment { Id = "EQ-PROJ-001", Name = "Máy chiếu" });
        await _db.SaveChangesAsync();

        var result = await _controller.Delete("EQ-PROJ-001");

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        Assert.That(await _db.Equipment.FindAsync("EQ-PROJ-001"), Is.Null);
    }

    [Test]
    public async Task Delete_Nonexistent_ReturnsNotFound()
    {
        var result = await _controller.Delete("EQ-NONEXIST");
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateStatus_ChangesStatusAndNote()
    {
        _db.Equipment.Add(new Equipment { Id = "EQ-PROJ-001", Name = "Máy chiếu", Status = EquipmentStatus.Available });
        await _db.SaveChangesAsync();

        await _controller.UpdateStatus("EQ-PROJ-001", EquipmentStatus.Broken, "Hỏng bóng đèn");

        var eq = await _db.Equipment.FindAsync("EQ-PROJ-001");
        Assert.Multiple(() =>
        {
            Assert.That(eq!.Status, Is.EqualTo(EquipmentStatus.Broken));
            Assert.That(eq.Note, Is.EqualTo("Hỏng bóng đèn"));
        });
    }

    [Test]
    public async Task UpdateStatus_Nonexistent_ReturnsNotFound()
    {
        var result = await _controller.UpdateStatus("EQ-NONEXIST", EquipmentStatus.Maintenance, null);
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Transfer_Get_SetsViewBag()
    {
        var room = new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 2", IsActive = true };
        _db.Rooms.Add(room);
        _db.Equipment.Add(new Equipment { Id = "EQ-PROJ-001", Name = "Máy chiếu" });
        await _db.SaveChangesAsync();

        var result = await _controller.Transfer("EQ-PROJ-001");

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewBag.EquipmentName, Is.EqualTo("Máy chiếu"));
        Assert.That(_controller.ViewBag.Rooms, Is.Not.Null);
    }

    [Test]
    public async Task Transfer_Post_MovesEquipment()
    {
        _db.Rooms.Add(new Room { Id = "RM-M-001", Name = "Phòng A", Capacity = 6, PricePerHour = 100_000, Location = "Tầng 2", IsActive = true });
        _db.Rooms.Add(new Room { Id = "RM-M-002", Name = "Phòng B", Capacity = 8, PricePerHour = 150_000, Location = "Tầng 2", IsActive = true });
        _db.Equipment.Add(new Equipment { Id = "EQ-PROJ-001", Name = "Máy chiếu" });
        _db.RoomEquipments.Add(new RoomEquipment { RoomId = "RM-M-001", EquipmentId = "EQ-PROJ-001" });
        await _db.SaveChangesAsync();

        var result = await _controller.Transfer("EQ-PROJ-001", "RM-M-002");

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var re = await _db.RoomEquipments.FirstAsync(re => re.EquipmentId == "EQ-PROJ-001");
        Assert.That(re.RoomId, Is.EqualTo("RM-M-002"));
    }

    [Test]
    public async Task Transfer_NonexistentEquipment_ReturnsNotFound()
    {
        var result = await _controller.Transfer("EQ-NONEXIST", "RM-M-001");
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
