using Co_working_Space.Models;

namespace Co_working_Space.Seeder.Data;

public static class RoomEquipmentData
{
    public static List<RoomEquipment> GetRoomEquipments() => new()
    {
        // Focus A
        new RoomEquipment { RoomId = "RM-M-001", EquipmentId = "EQP-003" },

        // Meeting B
        new RoomEquipment { RoomId = "RM-M-002", EquipmentId = "EQP-001" },
        new RoomEquipment { RoomId = "RM-M-002", EquipmentId = "EQP-002" },
        new RoomEquipment { RoomId = "RM-M-002", EquipmentId = "EQP-003" },

        // Hội thảo Grand
        new RoomEquipment { RoomId = "RM-L-001", EquipmentId = "EQP-001" },
        new RoomEquipment { RoomId = "RM-L-001", EquipmentId = "EQP-002" },
        new RoomEquipment { RoomId = "RM-L-001", EquipmentId = "EQP-004" },
        new RoomEquipment { RoomId = "RM-L-001", EquipmentId = "EQP-003" },

        // Executive VIP
        new RoomEquipment { RoomId = "RM-VIP-001", EquipmentId = "EQP-001" },
        new RoomEquipment { RoomId = "RM-VIP-001", EquipmentId = "EQP-002" },
        new RoomEquipment { RoomId = "RM-VIP-001", EquipmentId = "EQP-004" }
    };
}
