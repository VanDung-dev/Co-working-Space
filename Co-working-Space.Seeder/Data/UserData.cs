using Co_working_Space.Services;

namespace Co_working_Space.Seeder.Data;

public record UserSeedModel(string Email, string Password, string Role, string Id, decimal WalletBalance);

public static class UserData
{
    public static List<UserSeedModel> GetUsers() => new()
    {
        new("admin@coworking.com", "Admin@123", "Admin", IdGenerator.Next(IdGenerator.Admin), 0),
        new("staff@coworking.com", "Staff@123", "Staff", IdGenerator.Next(IdGenerator.Staff), 0),
        new("user1@coworking.com", "User@123", "User", IdGenerator.Next(IdGenerator.User), 2_000_000),
        new("user2@coworking.com", "User@123", "User", IdGenerator.Next(IdGenerator.User), 1_000_000),
    };
}
