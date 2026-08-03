using Co_working_Space.Models;

namespace Co_working_Space.Seeder.Data;

public static class RoomData
{
    public static List<Room> GetRooms() => new()
    {
        new Room
        {
            Id = "RM-M-001",
            Name = "Phòng Focus A",
            Capacity = 4,
            PricePerHour = 100_000,
            IsActive = true
        },
        new Room
        {
            Id = "RM-M-002",
            Name = "Phòng Meeting B",
            Capacity = 10,
            PricePerHour = 200_000,
            IsActive = true
        },
        new Room
        {
            Id = "RM-L-001",
            Name = "Phòng Hội thảo Grand",
            Capacity = 30,
            PricePerHour = 500_000,
            IsActive = true
        },
        new Room
        {
            Id = "RM-VIP-001",
            Name = "Phòng Executive VIP",
            Capacity = 8,
            PricePerHour = 350_000,
            IsActive = true
        }
    };
}
