using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Models.Enums;
using Co_working_Space.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    public BookingService(ApplicationDbContext context) => _context = context;

    public async Task<bool> HasOverlapAsync(string roomId, DateTime startTime, DateTime endTime, string? currentBookingId = null)
    {
        return await _context.Bookings.AnyAsync(b =>
            b.RoomId == roomId &&
            (currentBookingId == null || b.Id != currentBookingId) &&
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved) &&
            startTime < b.EndTime && endTime > b.StartTime);
    }

    public async Task<string?> CreateBookingAsync(CreateBookingViewModel model, string userId)
    {
        if (model.StartTime >= model.EndTime || model.StartTime <= DateTime.UtcNow)
            return null;

        var room = await _context.Rooms.FindAsync(model.RoomId);
        if (room == null || !room.IsActive) return null;

        var strategy = _context.Database.CreateExecutionStrategy();
        var bookingId = await strategy.ExecuteAsync(async () =>
        {
            using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            if (await HasOverlapAsync(model.RoomId, model.StartTime, model.EndTime))
                return null;

            var hours = (decimal)(model.EndTime - model.StartTime).TotalHours;
            var booking = new Booking
            {
                Id = IdGenerator.Next(IdGenerator.Booking),
                UserId = userId,
                RoomId = model.RoomId,
                Title = model.Title,
                Description = model.Description,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                TotalPrice = hours * room.PricePerHour,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return booking.Id;
        });

        return bookingId;
    }
}
