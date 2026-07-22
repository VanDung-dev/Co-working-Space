using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Services;

public class ApprovalService : IApprovalService
{
    private readonly ApplicationDbContext _context;
    public ApprovalService(ApplicationDbContext context) => _context = context;

    public async Task<List<Booking>> GetPendingAsync()
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.Status == BookingStatus.Pending)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(string bookingId, string approverId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null || booking.Status != BookingStatus.Pending)
            return (false, "Đơn không tồn tại hoặc không ở trạng thái chờ duyệt.");

        var wallet = await _context.Wallets.FindAsync(booking.UserId);
        if (wallet == null || wallet.Balance < booking.TotalPrice)
            return (false, $"Số dư không đủ (cần {booking.TotalPrice:N0}đ, hiện có {(wallet?.Balance ?? 0):N0}đ).");

        wallet.Balance -= booking.TotalPrice;
        booking.Status = BookingStatus.Approved;
        booking.PaymentStatus = PaymentStatus.Paid;
        booking.PaidAt = DateTime.UtcNow;

        _context.BookingApprovals.Add(new BookingApproval
        {
            Id = IdGenerator.Next(IdGenerator.Approval),
            BookingId = bookingId,
            ApproverId = approverId,
            Status = 1,
            ApprovedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> RejectAsync(string bookingId, string approverId, string reason)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null || booking.Status != BookingStatus.Pending) return false;

        if (booking.PaymentStatus == PaymentStatus.Paid)
        {
            var wallet = await _context.Wallets.FindAsync(booking.UserId);
            if (wallet != null)
                wallet.Balance += booking.TotalPrice;
            booking.PaymentStatus = PaymentStatus.Refunded;
        }

        booking.Status = BookingStatus.Rejected;
        _context.BookingApprovals.Add(new BookingApproval
        {
            Id = IdGenerator.Next(IdGenerator.Approval),
            BookingId = bookingId,
            ApproverId = approverId,
            Status = 2,
            Reason = reason,
            ApprovedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }
}
