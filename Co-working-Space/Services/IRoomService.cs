using Co_working_Space.Models;

namespace Co_working_Space.Services;

public interface IRoomService
{
    Task<List<Room>> SearchAsync(int? minCapacity, string? location, List<string>? equipment);
}
