using Co_working_Space.Data;
using Co_working_Space.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Areas.Admin.Controllers;

public class TopCustomerDto
{
    public string Email { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalSpent { get; set; }
}

public class TopWalletUserDto
{
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class RoomStatDto
{
    public string RoomName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public double TotalHours { get; set; }
    public decimal TotalRevenue { get; set; }
}

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index(int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var yearStart = new DateTime(targetYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var availableYears = Enumerable.Range(DateTime.UtcNow.Year - 3, 5).OrderByDescending(y => y).ToList();

        // KPI Metrics filtered by year
        var yearBookingsQuery = _context.Bookings.Where(b => b.CreatedAt >= yearStart && b.CreatedAt < yearEnd);

        var totalRevenue = await yearBookingsQuery
            .Where(b => b.Status == BookingStatus.Approved)
            .SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;

        var totalBookings = await yearBookingsQuery.CountAsync();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var monthlyBookings = await _context.Bookings
            .CountAsync(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd);

        var totalUsers = await _context.Users.CountAsync();

        var totalWalletBalance = await _context.Wallets
            .SumAsync(w => (decimal?)w.Balance) ?? 0m;

        var activeRoomsCount = await _context.Rooms
            .CountAsync(r => r.IsActive);

        // Status Counts for Charts
        var pendingCount = await yearBookingsQuery.CountAsync(b => b.Status == BookingStatus.Pending);
        var approvedCount = await yearBookingsQuery.CountAsync(b => b.Status == BookingStatus.Approved);
        var rejectedCount = await yearBookingsQuery.CountAsync(b => b.Status == BookingStatus.Rejected);
        var cancelledCount = await yearBookingsQuery.CountAsync(b => b.Status == BookingStatus.Cancelled);

        // Equipment Stats
        var eqAvailable = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Available);
        var eqMaintenance = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Maintenance);
        var eqBroken = await _context.Equipment.CountAsync(e => e.Status == EquipmentStatus.Broken);

        // Top Customers by Approved Bookings in selected year
        var approvedBookings = await yearBookingsQuery
            .Include(b => b.User)
            .Where(b => b.Status == BookingStatus.Approved)
            .ToListAsync();

        var topCustomers = approvedBookings
            .GroupBy(b => b.UserId)
            .Select(g => new TopCustomerDto
            {
                Email = g.First().User?.Email ?? "N/A",
                BookingCount = g.Count(),
                TotalSpent = g.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(c => c.BookingCount)
            .ThenByDescending(c => c.TotalSpent)
            .Take(5)
            .ToList();

        // Top Wallet Balances
        var topWalletUsers = await _context.Wallets
            .Include(w => w.User)
            .OrderByDescending(w => w.Balance)
            .Take(5)
            .Select(w => new TopWalletUserDto
            {
                Email = w.User != null ? w.User.Email ?? "N/A" : "N/A",
                Balance = w.Balance
            })
            .ToListAsync();

        // Top Rooms Stats in selected year
        var topRooms = approvedBookings
            .GroupBy(b => b.RoomId)
            .Select(g => new RoomStatDto
            {
                RoomName = g.First().Room?.Name ?? "Phòng không tên",
                Location = g.First().Room?.Location ?? "N/A",
                BookingCount = g.Count(),
                TotalHours = Math.Round(g.Sum(b => (b.EndTime - b.StartTime).TotalHours), 1),
                TotalRevenue = g.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(r => r.BookingCount)
            .ThenByDescending(r => r.TotalHours)
            .Take(5)
            .ToList();

        ViewBag.SelectedYear = targetYear;
        ViewBag.AvailableYears = availableYears;

        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalBookings = totalBookings;
        ViewBag.MonthlyBookings = monthlyBookings;
        ViewBag.TotalUsers = totalUsers;
        ViewBag.TotalWalletBalance = totalWalletBalance;
        ViewBag.ActiveRoomsCount = activeRoomsCount;

        ViewBag.PendingCount = pendingCount;
        ViewBag.ApprovedCount = approvedCount;
        ViewBag.RejectedCount = rejectedCount;
        ViewBag.CancelledCount = cancelledCount;

        ViewBag.EqAvailable = eqAvailable;
        ViewBag.EqMaintenance = eqMaintenance;
        ViewBag.EqBroken = eqBroken;

        ViewBag.TopCustomers = topCustomers;
        ViewBag.TopWalletUsers = topWalletUsers;
        ViewBag.TopRooms = topRooms;
        ViewBag.MostUsedRooms = topRooms;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var yearStart = new DateTime(targetYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var yearBookingsQuery = _context.Bookings.Where(b => b.CreatedAt >= yearStart && b.CreatedAt < yearEnd);

        var totalRevenue = await yearBookingsQuery
            .Where(b => b.Status == BookingStatus.Approved)
            .SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;

        var totalBookings = await yearBookingsQuery.CountAsync();
        var approvedCount = await yearBookingsQuery.CountAsync(b => b.Status == BookingStatus.Approved);

        var approvedBookings = await yearBookingsQuery
            .Include(b => b.User)
            .Include(b => b.Room)
            .Where(b => b.Status == BookingStatus.Approved)
            .ToListAsync();

        var topRooms = approvedBookings
            .GroupBy(b => b.RoomId)
            .Select(g => new
            {
                Name = g.First().Room?.Name ?? "Phòng không tên",
                Location = g.First().Room?.Location ?? "N/A",
                Count = g.Count(),
                Hours = Math.Round(g.Sum(b => (b.EndTime - b.StartTime).TotalHours), 1),
                Revenue = g.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(r => r.Count)
            .ToList();

        var topCustomers = approvedBookings
            .GroupBy(b => b.UserId)
            .Select(g => new
            {
                Email = g.First().User?.Email ?? "N/A",
                Count = g.Count(),
                Spent = g.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        var sb = new System.Text.StringBuilder();

        // Title
        sb.AppendLine($"\"=== BÁO CÁO TỔNG QUAN HỆ THỐNG CO-WORKING SPACE NĂM {targetYear} ===\"");
        sb.AppendLine("\"Chỉ số\",\"Giá trị\"");
        sb.AppendLine($"\"Năm báo cáo\",\"{targetYear}\"");
        sb.AppendLine($"\"Tổng doanh thu năm\",\"{totalRevenue:N0} đ\"");
        sb.AppendLine($"\"Tổng số đơn đặt\",\"{totalBookings}\"");
        sb.AppendLine($"\"Số đơn đã duyệt\",\"{approvedCount}\"");
        sb.AppendLine();

        // Room Stats
        sb.AppendLine($"\"=== THỐNG KÊ PHÒNG HỌP NĂM {targetYear} ===\"");
        sb.AppendLine("\"Tên phòng\",\"Vị trí\",\"Số lần đặt\",\"Tổng số giờ sử dụng\",\"Doanh thu phòng (đ)\"");
        foreach (var r in topRooms)
        {
            sb.AppendLine($"\"{r.Name}\",\"{r.Location}\",\"{r.Count}\",\"{r.Hours}\",\"{r.Revenue:N0}\"");
        }
        sb.AppendLine();

        // Customer Stats
        sb.AppendLine($"\"=== TOP KHÁCH HÀNG ĐẶT PHÒNG NĂM {targetYear} ===\"");
        sb.AppendLine("\"Email Khách hàng\",\"Số đơn đã duyệt\",\"Tổng chi tiêu (đ)\"");
        foreach (var c in topCustomers)
        {
            sb.AppendLine($"\"{c.Email}\",\"{c.Count}\",\"{c.Spent:N0}\"");
        }

        var csvBytes = System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();

        return File(csvBytes, "text/csv", $"Bao_Cao_Coworking_Space_Nam_{targetYear}.csv");
    }
}
