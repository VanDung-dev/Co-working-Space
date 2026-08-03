using Co_working_Space.Models;
using Co_working_Space.Models.Enums;

namespace Co_working_Space.Seeder.Data;

public static class EquipmentData
{
    public static List<Equipment> GetEquipment() => new()
    {
        new Equipment
        {
            Id = "EQP-001",
            Name = "Máy chiếu 4K Ultra HD",
            Description = "Máy chiếu độ phân giải cao 4K",
            Status = EquipmentStatus.Available
        },
        new Equipment
        {
            Id = "EQP-002",
            Name = "Micro hội nghị không dây",
            Description = "Micro đa hướng chống ồn",
            Status = EquipmentStatus.Available
        },
        new Equipment
        {
            Id = "EQP-003",
            Name = "Bảng trắng di động 1.2x2m",
            Description = "Bảng từ viền nhôm có bánh xe",
            Status = EquipmentStatus.Available
        },
        new Equipment
        {
            Id = "EQP-004",
            Name = "Hệ thống Video Conference 4K",
            Description = "Camera & soundbar trực tuyến",
            Status = EquipmentStatus.Available
        }
    };
}
