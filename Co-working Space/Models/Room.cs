using Co_working_Space.Models.Enums;

namespace Co_working_Space.Models;

public class Room
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}
