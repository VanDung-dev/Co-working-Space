using Co_working_Space.Models;

namespace Co_working_Space.Services;

public interface IApprovalService
{
    Task<List<Booking>> GetPendingAsync();
    Task<(bool Success, string? Error)> ApproveAsync(string bookingId, string approverId);
    Task<bool> RejectAsync(string bookingId, string approverId, string reason);
}
