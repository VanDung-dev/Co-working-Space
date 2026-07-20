# Backend Implementation (C# Code Mẫu)

> File này chứa toàn bộ code mẫu backend: Entity Models, Services, Controllers, Admin CRUD, RBAC. Xem `guildlines.md` cho tổng quan hệ thống.

---

## 1. ID trong Code

### 1.1. Entity Models — trường Id

```csharp
public class Booking
{
    public string Id { get; set; } = string.Empty;   // "BKG-20260720-001"
    public string UserId { get; set; } = string.Empty; // "USR-0001"
    public string RoomId { get; set; } = string.Empty; // "RM-S-001"
}

public class Room
{
    public string Id { get; set; } = string.Empty;   // "RM-S-001"
}

public class Equipment
{
    public string Id { get; set; } = string.Empty;   // "EQ-PROJ-002"
}
```

> `AspNetUsers.Id` mặc định là `string` trong ASP.NET Core Identity, hoàn toàn tương thích. Tất cả khóa chính dùng `NVARCHAR`.

### 1.2. Service sinh ID tự động

```csharp
public class IdGenerator
{
    private static readonly object _lock = new();
    private static readonly Dictionary<string, int> _counters = new();

    public static string Next(string prefix)
    {
        lock (_lock)
        {
            _counters.TryGetValue(prefix, out int current);
            _counters[prefix] = ++current;
            if (prefix.StartsWith("BKG-"))
            {
                var today = DateTime.UtcNow.ToString("yyyyMMdd");
                return $"{prefix}{today}-{current:D3}";
            }
            return $"{prefix}{current:D4}";
        }
    }

    public const string User     = "USR-";
    public const string Staff    = "STF-";
    public const string Admin    = "ADM-";
    public const string RoomSmall   = "RM-S-";
    public const string RoomMedium  = "RM-M-";
    public const string RoomLarge   = "RM-L-";
    public const string RoomVip     = "RM-V-";
    public const string Booking  = "BKG-";
    public const string Approval = "APR-";
    public const string EquipProjector   = "EQ-PROJ-";
    public const string EquipTV         = "EQ-TV-";
    public const string EquipMicrophone = "EQ-MIC-";
    public const string EquipWhiteboard = "EQ-WB-";
    public const string EquipSpeaker    = "EQ-SPK-";
    public const string EquipCamera     = "EQ-CAM-";
    public const string EquipCapture    = "EQ-CAP-";
}
```

> `_counters` dùng bộ nhớ trong — khi deploy thật, thay bằng sequence số từ DB.

---

## 2. Entity Models & ViewModel (`Models/`)

### BookingStatus

```csharp
namespace RoomBookingApp.Models
{
    public enum BookingStatus
    {
        Pending = 0,   // Chờ duyệt
        Approved = 1,  // Đã duyệt
        Rejected = 2,  // Từ chối
        Cancelled = 3  // Đã hủy
    }
}
```

### EquipmentStatus

```csharp
namespace RoomBookingApp.Models
{
    public enum EquipmentStatus
    {
        Available = 0,   // Sẵn sàng
        Maintenance = 1, // Bảo trì
        Broken = 2       // Hỏng
    }
}
```

### Room

```csharp
namespace RoomBookingApp.Models
{
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
}
```

### Booking

```csharp
namespace RoomBookingApp.Models
{
    public class Booking
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Room? Room { get; set; }
    }
}
```

### BookingApproval

```csharp
namespace RoomBookingApp.Models
{
    public class BookingApproval
    {
        public string Id { get; set; } = string.Empty;
        public string BookingId { get; set; } = string.Empty;
        public string ApproverId { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Reason { get; set; }
        public DateTime ApprovedAt { get; set; }
        public Booking Booking { get; set; } = null!;
    }
}
```

### Equipment & RoomEquipment

```csharp
namespace RoomBookingApp.Models
{
    public class Equipment
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
        public string? Note { get; set; }
        public ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
    }

    public class RoomEquipment
    {
        public string RoomId { get; set; } = string.Empty;
        public Room Room { get; set; } = null!;
        public string EquipmentId { get; set; } = string.Empty;
        public Equipment Equipment { get; set; } = null!;
    }
}
```

### ViewModels

```csharp
namespace RoomBookingApp.Models
{
    public class CreateBookingViewModel
    {
        public string RoomId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class RegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class ProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
```

### ApplicationDbContext

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RoomBookingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingApproval> BookingApprovals => Set<BookingApproval>();
        public DbSet<Equipment> Equipment => Set<Equipment>();
        public DbSet<RoomEquipment> RoomEquipments => Set<RoomEquipment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RoomEquipment>().HasKey(re => new { re.RoomId, re.EquipmentId });
        }
    }
}
```

---

## 3. BookingService — Chống trùng lịch + TOCTOU fix

> **Mẹo:** Hai khoảng thời gian $(A_{start}, A_{end})$ và $(B_{start}, B_{end})$ trùng nhau khi: $A_{start} < B_{end}$ **và** $A_{end} > B_{start}$.

```csharp
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;

namespace RoomBookingApp.Services
{
    public interface IBookingService
    {
        Task<bool> HasOverlapAsync(string roomId, DateTime startTime, DateTime endTime, string? currentBookingId = null);
        Task<string?> CreateBookingAsync(CreateBookingViewModel model, string userId);
    }

    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context) => _context = context;

        public async Task<bool> HasOverlapAsync(string roomId, DateTime startTime, DateTime endTime, string? currentBookingId = null)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                (currentBookingId == null || b.Id != currentBookingId) &&
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved) &&
                startTime < b.EndTime && endTime > b.StartTime);
        }

        public async Task<string?> CreateBookingAsync(CreateBookingViewModel model, string userId)
        {
            if (model.StartTime >= model.EndTime || model.StartTime <= DateTime.UtcNow)
                return null;

            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null || !room.IsActive) return null;

            var strategy = _context.Database.CreateExecutionStrategy();
            var bookingId = await strategy.ExecuteAsync(async () =>
            {
                using var tx = await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

                if (await HasOverlapAsync(model.RoomId, model.StartTime, model.EndTime))
                    return null;

                var hours = (decimal)(model.EndTime - model.StartTime).TotalHours;
                var booking = new Booking
                {
                    Id = IdGenerator.Next(IdGenerator.Booking),
                    UserId = userId,
                    RoomId = model.RoomId,
                    Title = model.Title,
                    Description = model.Description,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    TotalPrice = hours * room.PricePerHour,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return booking.Id;
            });

            return bookingId;
        }
    }
}
```

---

## 4. RoomService — Tra cứu & Lọc phòng

```csharp
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;

namespace RoomBookingApp.Services
{
    public interface IRoomService
    {
        Task<List<Room>> SearchAsync(int? minCapacity, string? location, List<string>? equipment);
    }

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
                query = query.Where(r => r.RoomEquipments
                    .Any(re => equipment.Contains(re.Equipment.Name)));

            return await query.OrderBy(r => r.Name).ToListAsync();
        }
    }
}
```

---

## 5. ApprovalService — Duyệt / Từ chối đặt phòng

```csharp
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;

namespace RoomBookingApp.Services
{
    public interface IApprovalService
    {
        Task<List<Booking>> GetPendingAsync();
        Task<bool> ApproveAsync(string bookingId, string approverId);
        Task<bool> RejectAsync(string bookingId, string approverId, string reason);
    }

    public class ApprovalService : IApprovalService
    {
        private readonly ApplicationDbContext _context;
        public ApprovalService(ApplicationDbContext context) => _context = context;

        public async Task<List<Booking>> GetPendingAsync()
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> ApproveAsync(string bookingId, string approverId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null || booking.Status != BookingStatus.Pending) return false;

            booking.Status = BookingStatus.Approved;
            _context.BookingApprovals.Add(new BookingApproval
            {
                Id = IdGenerator.Next(IdGenerator.Approval),
                BookingId = bookingId,
                ApproverId = approverId,
                Status = 1,
                ApprovedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(string bookingId, string approverId, string reason)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null || booking.Status != BookingStatus.Pending) return false;

            booking.Status = BookingStatus.Rejected;
            _context.BookingApprovals.Add(new BookingApproval
            {
                Id = IdGenerator.Next(IdGenerator.Approval),
                BookingId = bookingId,
                ApproverId = approverId,
                Status = 2,
                Reason = reason,
                ApprovedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
```

---

## 6. AccountController — Đăng ký / Đăng nhập / Hồ sơ

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoomBookingApp.Models;

namespace RoomBookingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new IdentityUser
            {
                Id = IdGenerator.Next(IdGenerator.User),
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded) return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Đăng nhập không hợp lệ.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(new ProfileViewModel
            {
                Email = user!.Email!,
                PhoneNumber = user.PhoneNumber ?? ""
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            user!.PhoneNumber = model.PhoneNumber;
            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }
    }
}
```

---

## 7. RoomController — Tra cứu phòng (User)

```csharp
using Microsoft.AspNetCore.Mvc;
using RoomBookingApp.Services;

namespace RoomBookingApp.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        public RoomController(IRoomService roomService) => _roomService = roomService;

        [HttpGet]
        public async Task<IActionResult> Index(int? minCapacity, string? location, string? equipment)
        {
            var equipList = string.IsNullOrWhiteSpace(equipment) ? null : equipment.Split(',').ToList();
            var rooms = await _roomService.SearchAsync(minCapacity, location, equipList);
            return View(rooms);
        }
    }
}
```

---

## 8. BookingController — Đặt phòng + Lịch sử + Hủy (User)

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;
using RoomBookingApp.Services;

namespace RoomBookingApp.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public BookingController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(string roomId)
        {
            ViewBag.RoomId = roomId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var bookingId = await _bookingService.CreateBookingAsync(model, userId);

            if (bookingId != null)
            {
                TempData["SuccessMessage"] = $"Tạo yêu cầu đặt phòng thành công! Mã đơn: {bookingId}";
                return RedirectToAction("MyBookings");
            }

            ModelState.AddModelError("", "Thời gian đặt phòng không hợp lệ hoặc đã bị trùng.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null) return NotFound();
            if (booking.Status != BookingStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn ở trạng thái Chờ duyệt.";
                return RedirectToAction("MyBookings");
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã hủy đơn đặt phòng.";
            return RedirectToAction("MyBookings");
        }
    }
}
```

---

## 9. Admin — RoomController (CRUD phòng + trạng thái)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;

namespace RoomBookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RoomController(ApplicationDbContext context) => _context = context;

        // --- Staff & Admin: xem danh sách, bật/tắt bảo trì ---

        [HttpGet]
        public async Task<IActionResult> Index()
            => View(await _context.Rooms.OrderBy(r => r.Name).ToListAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            room.IsActive = !room.IsActive;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = room.IsActive
                ? $"Phòng {room.Name} đã hoạt động trở lại."
                : $"Phòng {room.Name} đã chuyển sang Bảo trì.";
            return RedirectToAction("Index");
        }

        // --- Chỉ Admin: CRUD + gán thiết bị ---

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(room);
            room.Id = room.Capacity switch
            {
                <= 4 => IdGenerator.Next(IdGenerator.RoomSmall),
                <= 8 => IdGenerator.Next(IdGenerator.RoomMedium),
                <= 15 => IdGenerator.Next(IdGenerator.RoomLarge),
                _ => IdGenerator.Next(IdGenerator.RoomVip)
            };

            if (imageFile != null)
                room.ImageUrl = await SaveImageAsync(imageFile);

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã thêm phòng {room.Id}.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            return room == null ? NotFound() : View(room);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Room room, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(room);
            var existing = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == room.Id);
            if (existing == null) return NotFound();

            if (imageFile != null)
                room.ImageUrl = await SaveImageAsync(imageFile);
            else
                room.ImageUrl = existing.ImageUrl;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật phòng.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ManageEquipment(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            var allEquipment = await _context.Equipment.ToListAsync();
            var assignedIds = await _context.RoomEquipments
                .Where(re => re.RoomId == id)
                .Select(re => re.EquipmentId)
                .ToListAsync();

            ViewBag.RoomName = room.Name;
            return View(Tuple.Create(allEquipment, assignedIds));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageEquipment(string roomId, List<string> equipmentIds)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return NotFound();

            var existing = await _context.RoomEquipments.Where(re => re.RoomId == roomId).ToListAsync();
            _context.RoomEquipments.RemoveRange(existing);

            if (equipmentIds != null)
            {
                foreach (var eid in equipmentIds)
                    _context.RoomEquipments.Add(new RoomEquipment { RoomId = roomId, EquipmentId = eid });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật thiết bị cho phòng.";
            return RedirectToAction("Index");
        }

        // --- Helper ---

        // ponytail: lưu 1 file, tạo thư mục nếu chưa có. Nâng cấp lên blob storage khi scale.
        private static async Task<string?> SaveImageAsync(IFormFile file)
        {
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "rooms");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/rooms/{fileName}";
        }
    }
}
```

---

## 10. Admin — EquipmentController (CRUD thiết bị)

```csharp
namespace RoomBookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class EquipmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EquipmentController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index()
            => View(await _context.Equipment.OrderBy(e => e.Name).ToListAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest();
            var prefix = name.ToUpperInvariant() switch
            {
                string n when n.Contains("MÁY CHIẾU") || n.Contains("PROJECTOR") => IdGenerator.EquipProjector,
                string n when n.Contains("TIVI") || n.Contains("TV") => IdGenerator.EquipTV,
                string n when n.Contains("MICRO") => IdGenerator.EquipMicrophone,
                string n when n.Contains("BẢNG") || n.Contains("WHITEBOARD") => IdGenerator.EquipWhiteboard,
                string n when n.Contains("LOA") || n.Contains("SPEAKER") => IdGenerator.EquipSpeaker,
                string n when n.Contains("CAMERA") => IdGenerator.EquipCamera,
                _ => IdGenerator.EquipCapture
            };
            var equipment = new Equipment
            {
                Id = IdGenerator.Next(prefix),
                Name = name,
                Description = description,
                Status = EquipmentStatus.Available
            };
            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null) return NotFound();
            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, EquipmentStatus status, string? note)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null) return NotFound();
            equipment.Status = status;
            equipment.Note = note;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái {equipment.Name} ({status}).";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Transfer(string id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null) return NotFound();

            ViewBag.EquipmentName = equipment.Name;
            ViewBag.CurrentRoom = await _context.RoomEquipments
                .Where(re => re.EquipmentId == id)
                .Select(re => re.Room.Name)
                .FirstOrDefaultAsync() ?? "(chưa gán)";

            ViewBag.Rooms = await _context.Rooms.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(string equipmentId, string targetRoomId)
        {
            var equipment = await _context.Equipment.FindAsync(equipmentId);
            if (equipment == null) return NotFound();

            var existing = await _context.RoomEquipments
                .Where(re => re.EquipmentId == equipmentId)
                .ToListAsync();
            _context.RoomEquipments.RemoveRange(existing);

            _context.RoomEquipments.Add(new RoomEquipment
            {
                RoomId = targetRoomId,
                EquipmentId = equipmentId
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã chuyển {equipment.Name} sang phòng mới.";
            return RedirectToAction("Index");
        }
    }
}
```

---

## 11. Admin — BookingController (Duyệt đặt phòng)

```csharp
namespace RoomBookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class BookingController : Controller
    {
        private readonly IApprovalService _approvalService;
        public BookingController(IApprovalService approvalService) => _approvalService = approvalService;

        [HttpGet]
        public async Task<IActionResult> Pending()
            => View(await _approvalService.GetPendingAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _approvalService.ApproveAsync(id, approverId);
            if (!result) return NotFound();
            TempData["SuccessMessage"] = "Đã duyệt đơn đặt phòng.";
            return RedirectToAction("Pending");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, string reason)
        {
            var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _approvalService.RejectAsync(id, approverId, reason);
            if (!result) return NotFound();
            TempData["SuccessMessage"] = "Đã từ chối đơn đặt phòng.";
            return RedirectToAction("Pending");
        }
    }
}
```

---

## 12. Admin — UserController (Reset mật khẩu)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;

namespace RoomBookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(UserManager<IdentityUser> userManager) => _userManager = userManager;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var u in users)
                userRoles[u.Id] = await _userManager.GetRolesAsync(u);

            // Staff chỉ thấy User, Admin thấy tất cả
            if (User.IsInRole("Staff"))
            {
                users = users.Where(u => userRoles[u.Id].Contains("User")).ToList();
                userRoles = userRoles.Where(kv => users.Any(u => u.Id == kv.Key))
                                     .ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (User.IsInRole("Staff") && !roles.Contains("User"))
                return Forbid();

            ViewBag.UserEmail = user.Email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (User.IsInRole("Staff") && !roles.Contains("User"))
                return Forbid();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
                ViewBag.UserEmail = user.Email;
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Đã reset mật khẩu cho {user.Email}.";
                return RedirectToAction("Index");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            ViewBag.UserEmail = user.Email;
            return View();
        }
    }
}
```

---

## 13. Admin — DashboardController (Thống kê)

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;

namespace RoomBookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var monthlyBookings = await _context.Bookings
                .CountAsync(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd);

            var mostUsedRooms = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Approved)
                .GroupBy(b => b.RoomId)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(_context.Rooms, x => x.RoomId, r => r.Id, (x, r) => new { r.Name, x.Count })
                .ToListAsync();

            ViewBag.MonthlyBookings = monthlyBookings;
            ViewBag.MostUsedRooms = mostUsedRooms;
            return View();
        }
    }
}
```

---

## 14. Program.cs — Cấu hình Identity + RBAC + Seed

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", p => p.RequireRole("Staff", "Admin"));
});

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    foreach (var role in new[] { "Admin", "Staff", "User" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    if (await userManager.FindByEmailAsync("admin@coworking.com") == null)
    {
        var admin = new IdentityUser
        {
            Id = IdGenerator.Next(IdGenerator.Admin),
            UserName = "admin@coworking.com",
            Email = "admin@coworking.com"
        };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
```

---

## 15. xUnit — Unit Test mẫu cho BookingService

> Xem thêm `guildlines.database.md` cho DDL. `guildlines.testcase.md` cho 52 test case tổng thể.

```csharp
using RoomBookingApp.Models;
using RoomBookingApp.Services;
using Xunit;

namespace RoomBookingApp.Tests
{
    public class OverlapLogicTests
    {
        private readonly BookingService _service;

        // ponytail: dùng in-memory DB cho unit test, thay bằng mock DB khi cần isolation
        private ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
                .Options;
            var db = new ApplicationDbContext(options);

            db.Rooms.Add(new Room
            {
                Id = "RM-M-001",
                Name = "Phòng A",
                Capacity = 6,
                PricePerHour = 100_000,
                IsActive = true
            });

            db.Bookings.Add(new Booking
            {
                Id = "BKG-20260720-001",
                RoomId = "RM-M-001",
                UserId = "USR-0001",
                StartTime = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
                Status = BookingStatus.Approved
            });

            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task HasOverlapAsync_WhenOverlapExists_ReturnsTrue()
        {
            var db = CreateDbContext();
            var service = new BookingService(db);

            var result = await service.HasOverlapAsync(
                "RM-M-001",
                new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 20, 10, 30, 0, DateTimeKind.Utc));

            Assert.True(result);
        }

        [Fact]
        public async Task HasOverlapAsync_WhenNoOverlap_ReturnsFalse()
        {
            var db = CreateDbContext();
            var service = new BookingService(db);

            var result = await service.HasOverlapAsync(
                "RM-M-001",
                new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc));

            Assert.False(result);
        }

        [Fact]
        public async Task HasOverlapAsync_ExcludesCancelledBookings()
        {
            var db = CreateDbContext();
            db.Bookings.Add(new Booking
            {
                Id = "BKG-20260720-002",
                RoomId = "RM-M-001",
                UserId = "USR-0002",
                StartTime = new DateTime(2026, 7, 20, 14, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2026, 7, 20, 15, 0, 0, DateTimeKind.Utc),
                Status = BookingStatus.Cancelled
            });
            db.SaveChanges();

            var service = new BookingService(db);
            var result = await service.HasOverlapAsync(
                "RM-M-001",
                new DateTime(2026, 7, 20, 14, 30, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 20, 15, 30, 0, DateTimeKind.Utc));

            Assert.False(result);
        }
    }
}
```
