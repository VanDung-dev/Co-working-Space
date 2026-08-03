using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Co_working_Space.Services;

namespace Co_working_Space.Seeder.Data;

public static class BookingData
{
    public static (List<Booking> Bookings, List<BookingApproval> Approvals) GetBookingsAndApprovals(
        string user1Id,
        string user2Id,
        string staffId)
    {
        var now = DateTime.UtcNow;
        var bookings = new List<Booking>
        {
            new Booking
            {
                Id = "BKG-20260803-001",
                RoomId = "RM-M-002",
                UserId = user1Id,
                Title = "Họp Kế hoạch Quý 3",
                Description = "Cần máy chiếu và bảng trắng",
                StartTime = now.AddHours(2),
                EndTime = now.AddHours(4),
                TotalPrice = 400_000,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid
            },
            new Booking
            {
                Id = "BKG-20260803-002",
                RoomId = "RM-L-001",
                UserId = user2Id,
                Title = "Hội thảo Chia sẻ Công nghệ",
                Description = "Sử dụng hệ thống Video Conference",
                StartTime = now.AddDays(1).AddHours(9 - now.Hour),
                EndTime = now.AddDays(1).AddHours(11 - now.Hour),
                TotalPrice = 1_000_000,
                Status = BookingStatus.Approved,
                PaymentStatus = PaymentStatus.Paid,
                PaidAt = now
            },
            new Booking
            {
                Id = "BKG-20260803-003",
                RoomId = "RM-VIP-001",
                UserId = user1Id,
                Title = "Họp Đối tác Chiến lược",
                Description = "Phòng họp VIP",
                StartTime = now.AddDays(-1),
                EndTime = now.AddDays(-1).AddHours(2),
                TotalPrice = 700_000,
                Status = BookingStatus.Rejected,
                PaymentStatus = PaymentStatus.Unpaid
            }
        };

        var approvals = new List<BookingApproval>
        {
            new BookingApproval
            {
                Id = IdGenerator.Next(IdGenerator.Approval),
                BookingId = "BKG-20260803-002",
                ApproverId = staffId,
                Status = 1,
                ApprovedAt = now
            },
            new BookingApproval
            {
                Id = IdGenerator.Next(IdGenerator.Approval),
                BookingId = "BKG-20260803-003",
                ApproverId = staffId,
                Status = 2,
                Reason = "Trùng lịch bảo trì phòng",
                ApprovedAt = now
            }
        };

        return (bookings, approvals);
    }
}
