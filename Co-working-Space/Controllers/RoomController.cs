using Co_working_Space.Services;
using Microsoft.AspNetCore.Mvc;

namespace Co_working_Space.Controllers;

public class RoomController : Controller
{
    private readonly IRoomService _roomService;
    public RoomController(IRoomService roomService) => _roomService = roomService;

    [HttpGet]
    public async Task<IActionResult> Index(int? minCapacity, string? location, string? equipment)
    {
        var equipList = string.IsNullOrWhiteSpace(equipment) ? null : equipment.Split(',').ToList();
        var rooms = await _roomService.SearchAsync(minCapacity, location, equipList);
        return View(rooms);
    }
}
