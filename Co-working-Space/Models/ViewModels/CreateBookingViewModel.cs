namespace Co_working_Space.Models.ViewModels;

public class CreateBookingViewModel
{
    public string RoomId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
