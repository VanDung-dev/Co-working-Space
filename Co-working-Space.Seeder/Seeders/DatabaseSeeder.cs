using Co_working_Space.Data;
using Co_working_Space.Models;
using Co_working_Space.Seeder.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Seeder.Seeders;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public DatabaseSeeder(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync(bool cleanMode)
    {
        Console.WriteLine("\n⏳ Đang kết nối và kiểm tra Database...");
        await _db.Database.EnsureCreatedAsync();

        if (cleanMode)
        {
            await CleanDatabaseAsync();
        }

        Console.WriteLine("\n🌱 Bắt đầu tạo dữ liệu mẫu...");

        await SeedRolesAsync();
        var userDict = await SeedUsersAndWalletsAsync();
        await SeedEquipmentAsync();
        await SeedRoomsAsync();
        await SeedRoomEquipmentsAsync();
        await SeedBookingsAndApprovalsAsync(userDict);

        PrintSummary();
    }

    private async Task CleanDatabaseAsync()
    {
        Console.WriteLine("🧹 Đang dọn dẹp toàn bộ dữ liệu cũ trong Database...");
        _db.BookingApprovals.RemoveRange(_db.BookingApprovals);
        _db.Bookings.RemoveRange(_db.Bookings);
        _db.RoomEquipments.RemoveRange(_db.RoomEquipments);
        _db.Equipment.RemoveRange(_db.Equipment);
        _db.Rooms.RemoveRange(_db.Rooms);
        _db.Wallets.RemoveRange(_db.Wallets);
        await _db.SaveChangesAsync();

        var existingUsers = await _userManager.Users.ToListAsync();
        foreach (var u in existingUsers)
        {
            await _userManager.DeleteAsync(u);
        }
        Console.WriteLine("✅ Đã dọn dẹp sạch dữ liệu cũ.");
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Staff", "User" };
        foreach (var r in roles)
        {
            if (!await _roleManager.RoleExistsAsync(r))
            {
                await _roleManager.CreateAsync(new IdentityRole(r));
                Console.WriteLine($"  + Role created: {r}");
            }
        }
    }

    private async Task<Dictionary<string, IdentityUser>> SeedUsersAndWalletsAsync()
    {
        var userDict = new Dictionary<string, IdentityUser>();
        var userSeeds = UserData.GetUsers();

        foreach (var seed in userSeeds)
        {
            var existingUser = await _userManager.FindByEmailAsync(seed.Email);
            if (existingUser == null)
            {
                var user = new IdentityUser
                {
                    Id = seed.Id,
                    UserName = seed.Email,
                    Email = seed.Email,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, seed.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, seed.Role);
                    Console.WriteLine($"  + User created: {seed.Email} (Role: {seed.Role})");
                    userDict[seed.Email] = user;

                    if (seed.WalletBalance > 0 && !await _db.Wallets.AnyAsync(w => w.UserId == user.Id))
                    {
                        _db.Wallets.Add(new Wallet { UserId = user.Id, Balance = seed.WalletBalance });
                        Console.WriteLine($"    └─ Wallet initialized: {seed.WalletBalance:N0} đ");
                    }
                }
                else
                {
                    Console.WriteLine($"  ❌ Lỗi khi tạo user {seed.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                userDict[seed.Email] = existingUser;
            }
        }
        await _db.SaveChangesAsync();
        return userDict;
    }

    private async Task SeedEquipmentAsync()
    {
        var equipmentList = EquipmentData.GetEquipment();
        foreach (var eq in equipmentList)
        {
            if (!await _db.Equipment.AnyAsync(e => e.Id == eq.Id))
            {
                _db.Equipment.Add(eq);
                Console.WriteLine($"  + Equipment added: {eq.Name}");
            }
        }
        await _db.SaveChangesAsync();
    }

    private async Task SeedRoomsAsync()
    {
        var roomList = RoomData.GetRooms();
        foreach (var room in roomList)
        {
            if (!await _db.Rooms.AnyAsync(r => r.Id == room.Id))
            {
                _db.Rooms.Add(room);
                Console.WriteLine($"  + Room added: {room.Name} ({room.Capacity} người - {room.PricePerHour:N0}đ/h)");
            }
        }
        await _db.SaveChangesAsync();
    }

    private async Task SeedRoomEquipmentsAsync()
    {
        var roomEquipments = RoomEquipmentData.GetRoomEquipments();
        foreach (var re in roomEquipments)
        {
            if (!await _db.RoomEquipments.AnyAsync(r => r.RoomId == re.RoomId && r.EquipmentId == re.EquipmentId))
            {
                _db.RoomEquipments.Add(re);
            }
        }
        await _db.SaveChangesAsync();
        Console.WriteLine("  + Gán thiết bị vào các phòng họp hoàn tất.");
    }

    private async Task SeedBookingsAndApprovalsAsync(Dictionary<string, IdentityUser> userDict)
    {
        if (userDict.TryGetValue("user1@coworking.com", out var u1) &&
            userDict.TryGetValue("user2@coworking.com", out var u2) &&
            userDict.TryGetValue("staff@coworking.com", out var staffUser))
        {
            var (bookings, approvals) = BookingData.GetBookingsAndApprovals(u1.Id, u2.Id, staffUser.Id);

            foreach (var b in bookings)
            {
                if (!await _db.Bookings.AnyAsync(bk => bk.Id == b.Id))
                {
                    _db.Bookings.Add(b);
                    Console.WriteLine($"  + Booking added: {b.Id} [{b.Title}] ({b.Status})");
                }
            }
            await _db.SaveChangesAsync();

            foreach (var app in approvals)
            {
                if (!await _db.BookingApprovals.AnyAsync(a => a.BookingId == app.BookingId))
                {
                    _db.BookingApprovals.Add(app);
                }
            }
            await _db.SaveChangesAsync();
        }
    }

    private static void PrintSummary()
    {
        Console.WriteLine("\n=================================================");
        Console.WriteLine(" 🎉 TẠO DỮ LIỆU MẪU THÀNH CÔNG!");
        Console.WriteLine("=================================================");
        Console.WriteLine("📌 Danh sách tài khoản thử nghiệm:");
        Console.WriteLine("  1. Admin:  admin@coworking.com  / Admin@123");
        Console.WriteLine("  2. Staff:  staff@coworking.com  / Staff@123");
        Console.WriteLine("  3. User 1: user1@coworking.com  / User@123  (Ví: 2.000.000đ)");
        Console.WriteLine("  4. User 2: user2@coworking.com  / User@123  (Ví: 1.000.000đ)");
        Console.WriteLine("=================================================\n");
    }
}
