namespace Co_working_Space.Models;

public class BookingApproval
{
    public string Id { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string ApproverId { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Reason { get; set; }
    public DateTime ApprovedAt { get; set; }
    public Booking Booking { get; set; } = null!;
}
