using Co_working_Space.Models.Enums;

namespace Co_working_Space.Models;

public class Equipment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    public string? Note { get; set; }
    public ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}
