namespace Co_working_Space.Models;

public class RoomEquipment
{
    public string RoomId { get; set; } = string.Empty;
    public Room Room { get; set; } = null!;
    public string EquipmentId { get; set; } = string.Empty;
    public Equipment Equipment { get; set; } = null!;
}
