using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class RoomController : Controller
{
    private readonly ApplicationDbContext _context;
    public RoomController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await _context.Rooms.OrderBy(r => r.Name).ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        room.IsActive = !room.IsActive;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = room.IsActive
            ? $"Phòng {room.Name} đã hoạt động trở lại."
            : $"Phòng {room.Name} đã chuyển sang Bảo trì.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room room, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(room);
        room.Id = room.Capacity switch
        {
            <= 4 => IdGenerator.Next(IdGenerator.RoomSmall),
            <= 8 => IdGenerator.Next(IdGenerator.RoomMedium),
            <= 15 => IdGenerator.Next(IdGenerator.RoomLarge),
            _ => IdGenerator.Next(IdGenerator.RoomVip)
        };

        if (imageFile != null)
            room.ImageUrl = await SaveImageAsync(imageFile);

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Đã thêm phòng {room.Id}.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var room = await _context.Rooms.FindAsync(id);
        return room == null ? NotFound() : View(room);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Room room, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(room);
        var existing = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == room.Id);
        if (existing == null) return NotFound();

        if (imageFile != null)
            room.ImageUrl = await SaveImageAsync(imageFile);
        else
            room.ImageUrl = existing.ImageUrl;

        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã cập nhật phòng.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> ManageEquipment(string id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        var allEquipment = await _context.Equipment.ToListAsync();
        var assignedIds = await _context.RoomEquipments
            .Where(re => re.RoomId == id)
            .Select(re => re.EquipmentId)
            .ToListAsync();

        ViewBag.RoomName = room.Name;
        return View(Tuple.Create(allEquipment, assignedIds));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageEquipment(string roomId, List<string> equipmentIds)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null) return NotFound();

        var existing = await _context.RoomEquipments.Where(re => re.RoomId == roomId).ToListAsync();
        _context.RoomEquipments.RemoveRange(existing);

        if (equipmentIds != null)
        {
            foreach (var eid in equipmentIds)
                _context.RoomEquipments.Add(new RoomEquipment { RoomId = roomId, EquipmentId = eid });
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã cập nhật thiết bị cho phòng.";
        return RedirectToAction("Index");
    }

    private static async Task<string?> SaveImageAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "rooms");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/rooms/{fileName}";
    }
}
