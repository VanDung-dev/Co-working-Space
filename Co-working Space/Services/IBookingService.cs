using Co_working_Space.Models.ViewModels;

namespace Co_working_Space.Services;

public interface IBookingService
{
    Task<bool> HasOverlapAsync(string roomId, DateTime startTime, DateTime endTime, string? currentBookingId = null);
    Task<string?> CreateBookingAsync(CreateBookingViewModel model, string userId);
}
