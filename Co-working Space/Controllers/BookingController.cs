using System.Security.Claims;
using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Co_working_Space.Models.ViewModels;
using Co_working_Space.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly ApplicationDbContext _context;

    public BookingController(IBookingService bookingService, ApplicationDbContext context)
    {
        _bookingService = bookingService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Create(string roomId)
    {
        ViewBag.RoomId = roomId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var bookingId = await _bookingService.CreateBookingAsync(model, userId);

        if (bookingId != null)
        {
            TempData["SuccessMessage"] = $"Tạo yêu cầu đặt phòng thành công! Mã đơn: {bookingId}";
            return RedirectToAction("MyBookings");
        }

        ModelState.AddModelError("", "Thời gian đặt phòng không hợp lệ hoặc đã bị trùng.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (booking == null) return NotFound();
        if (booking.Status != BookingStatus.Pending)
        {
            TempData["ErrorMessage"] = "Chỉ có thể hủy đơn ở trạng thái Chờ duyệt.";
            return RedirectToAction("MyBookings");
        }

        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã hủy đơn đặt phòng.";
        return RedirectToAction("MyBookings");
    }
}
