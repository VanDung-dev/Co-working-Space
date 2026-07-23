using Co_working_Space.Data;
using Co_working_Space.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var monthlyBookings = await _context.Bookings
            .CountAsync(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd);

        var mostUsedRooms = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Approved)
            .GroupBy(b => b.RoomId)
            .Select(g => new { RoomId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .Join(_context.Rooms, x => x.RoomId, r => r.Id, (x, r) => new { r.Name, x.Count })
            .ToListAsync();

        ViewBag.MonthlyBookings = monthlyBookings;
        ViewBag.MostUsedRooms = mostUsedRooms;
        return View();
    }
}
