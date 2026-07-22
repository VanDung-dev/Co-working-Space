using Co_working_Space.Data;
using Co_working_Space.Models;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Services;

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;
    public RoomService(ApplicationDbContext context) => _context = context;

    public async Task<List<Room>> SearchAsync(int? minCapacity, string? location, List<string>? equipment)
    {
        var query = _context.Rooms
            .Include(r => r.RoomEquipments)
            .ThenInclude(re => re.Equipment)
            .Where(r => r.IsActive);

        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(r => r.Location.Contains(location));
        if (equipment is { Count: > 0 })
            query = query.Where(r => r.RoomEquipments.Any(re => equipment.Contains(re.Equipment.Name)));

        return await query.OrderBy(r => r.Name).ToListAsync();
    }
}
