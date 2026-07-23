using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class EquipmentController : Controller
{
    private readonly ApplicationDbContext _context;
    public EquipmentController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await _context.Equipment.Include(e => e.RoomEquipments).ThenInclude(re => re.Room).OrderBy(e => e.Name).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest();
        var prefix = name.ToUpperInvariant() switch
        {
            string n when n.Contains("MÁY CHIẾU") || n.Contains("PROJECTOR") => IdGenerator.EquipProjector,
            string n when n.Contains("TIVI") || n.Contains("TV") => IdGenerator.EquipTV,
            string n when n.Contains("MICRO") => IdGenerator.EquipMicrophone,
            string n when n.Contains("BẢNG") || n.Contains("WHITEBOARD") => IdGenerator.EquipWhiteboard,
            string n when n.Contains("LOA") || n.Contains("SPEAKER") => IdGenerator.EquipSpeaker,
            string n when n.Contains("CAMERA") => IdGenerator.EquipCamera,
            _ => IdGenerator.EquipCapture
        };
        var equipment = new Equipment
        {
            Id = IdGenerator.Next(prefix),
            Name = name,
            Description = description,
            Status = EquipmentStatus.Available
        };
        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var equipment = await _context.Equipment.FindAsync(id);
        if (equipment == null) return NotFound();
        _context.Equipment.Remove(equipment);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, EquipmentStatus status, string? note)
    {
        var equipment = await _context.Equipment.FindAsync(id);
        if (equipment == null) return NotFound();
        equipment.Status = status;
        equipment.Note = note;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Đã cập nhật trạng thái {equipment.Name} ({status}).";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Transfer(string id)
    {
        var equipment = await _context.Equipment.FindAsync(id);
        if (equipment == null) return NotFound();

        ViewBag.EquipmentName = equipment.Name;
        ViewBag.CurrentRoom = await _context.RoomEquipments
            .Where(re => re.EquipmentId == id)
            .Select(re => re.Room.Name)
            .FirstOrDefaultAsync() ?? "(chưa gán)";

        ViewBag.Rooms = await _context.Rooms.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(string equipmentId, string targetRoomId)
    {
        var equipment = await _context.Equipment.FindAsync(equipmentId);
        if (equipment == null) return NotFound();

        var existing = await _context.RoomEquipments
            .Where(re => re.EquipmentId == equipmentId)
            .ToListAsync();
        _context.RoomEquipments.RemoveRange(existing);

        _context.RoomEquipments.Add(new RoomEquipment
        {
            RoomId = targetRoomId,
            EquipmentId = equipmentId
        });

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Đã chuyển {equipment.Name} sang phòng mới.";
        return RedirectToAction("Index");
    }
}
