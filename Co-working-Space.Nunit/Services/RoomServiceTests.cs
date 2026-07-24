using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Services;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Nunit.Services;

[TestFixture]
public class RoomServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var db = new ApplicationDbContext(options);

        db.Rooms.AddRange(
            new Room { Id = "RM-S-001", Name = "Phòng Nhỏ", Location = "Tầng 1", Capacity = 4, PricePerHour = 50_000, IsActive = true },
            new Room { Id = "RM-M-001", Name = "Phòng Vừa", Location = "Tầng 2", Capacity = 8, PricePerHour = 100_000, IsActive = true },
            new Room { Id = "RM-L-001", Name = "Phòng Lớn", Location = "Tầng 2", Capacity = 15, PricePerHour = 200_000, IsActive = true },
            new Room { Id = "RM-V-001", Name = "Phòng VIP", Location = "Tầng 3", Capacity = 6, PricePerHour = 300_000, IsActive = false }
        );
        db.SaveChanges();
        return db;
    }

    [Test]
    public async Task SearchAsync_NoFilter_ReturnsAllActiveRooms()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, null, null);

        Assert.That(rooms, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task SearchAsync_ExcludesInactiveRooms()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, null, null);

        Assert.That(rooms.Any(r => r.Id == "RM-V-001"), Is.False);
    }

    [Test]
    public async Task SearchAsync_MinCapacity_ReturnsMatchingRooms()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(5, null, null);

        Assert.That(rooms, Has.All.Matches<Room>(r => r.Capacity >= 5));
        Assert.That(rooms, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task SearchAsync_LocationFilter_ReturnsMatching()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, "Tầng 2", null);

        Assert.That(rooms, Has.Count.EqualTo(2));
        Assert.That(rooms, Has.All.Matches<Room>(r => r.Location.Contains("Tầng 2")));
    }

    [Test]
    public async Task SearchAsync_LocationFilterPartialMatch()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, "Tầng", null);

        Assert.That(rooms, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task SearchAsync_ReturnsOrderedByName()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, null, null);

        for (int i = 1; i < rooms.Count; i++)
            Assert.That(string.Compare(rooms[i - 1].Name, rooms[i].Name, StringComparison.Ordinal), Is.LessThanOrEqualTo(0));
    }

    [Test]
    public async Task SearchAsync_WithEquipmentFilter()
    {
        using var db = CreateDb();
        var projector = new Equipment
        {
            Id = "EQ-PROJ-001", Name = "Máy chiếu",
            Status = Models.Enums.EquipmentStatus.Available
        };
        var tv = new Equipment
        {
            Id = "EQ-TV-001", Name = "Tivi",
            Status = Models.Enums.EquipmentStatus.Available
        };
        db.Equipment.AddRange(projector, tv);

        var room = await db.Rooms.FirstAsync();
        db.RoomEquipments.AddRange(
            new RoomEquipment { RoomId = room.Id, EquipmentId = projector.Id },
            new RoomEquipment { RoomId = room.Id, EquipmentId = tv.Id }
        );
        await db.SaveChangesAsync();

        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, null, ["Máy chiếu"]);

        Assert.That(rooms, Has.Count.EqualTo(1));
    }
}
