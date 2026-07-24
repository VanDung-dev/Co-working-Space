# NUnit Unit Testing — Co-working Space.Nunit

> File này chứa toàn bộ quy tắc và hướng dẫn viết unit test cho project `Co-working-Space.Nunit`. Xem `guildlines.md` (project gốc) cho tổng quan hệ thống.

---

## 1. Thiết lập Project

### 1.1. Thêm Project Reference vào Main Project

```xml
<!-- Co-working-Space.Nunit.csproj -->
<ItemGroup>
    <ProjectReference Include="..\Co-working-Space\Co-working-Space.csproj" />
</ItemGroup>
```

### 1.2. Packages đã có

| Package | Version | Mục đích |
|---------|---------|----------|
| `NUnit` | 4.3.2 | Test framework |
| `NUnit3TestAdapter` | 5.0.0 | Chạy test trong Visual Studio / CLI |
| `NUnit.Analyzers` | 4.7.0 | Phân tích code test |
| `Microsoft.NET.Test.Sdk` | 17.14.0 | CLI test runner |
| `coverlet.collector` | 6.0.4 | Code coverage |

### 1.3. Packages cần thêm

```xml
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.*" />
```

Chạy:
```bash
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

---

## 2. Cấu trúc & Quy tắc

### 2.1. Tổ chức File Test

```
Co-working-Space.Nunit/
├── Services/
│   ├── BookingServiceTests.cs
│   ├── RoomServiceTests.cs
│   └── ApprovalServiceTests.cs
├── Controllers/
│   ├── AccountControllerTests.cs
│   ├── BookingControllerTests.cs
│   └── RoomControllerTests.cs
├── Admin/
│   ├── AdminRoomControllerTests.cs
│   ├── AdminBookingControllerTests.cs
│   ├── AdminEquipmentControllerTests.cs
│   ├── AdminWalletControllerTests.cs
│   ├── AdminUserControllerTests.cs
│   └── AdminDashboardControllerTests.cs
└── Helpers/
    └── IdGeneratorTests.cs
```

### 2.2. NUnit Conventions

```csharp
namespace Co_working_Space.Nunit.Services;

[TestFixture]
public class BookingServiceTests
{
    private ApplicationDbContext _db = null!;
    private BookingService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        _db = new ApplicationDbContext(options);
        _service = new BookingService(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task HasOverlapAsync_WhenOverlapExists_ReturnsTrue()
    {
        SeedRoomAndBooking();

        var result = await _service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 10, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.True);
    }
}
```

### 2.3. Đặt Tên Test

```
{MethodName}_{Scenario}_{ExpectedResult}
```

Ví dụ: `HasOverlapAsync_WhenTimeRangeTouchesAtEnd_ReturnsFalse`

### 2.4. Categories

```csharp
[Test]
[Category("Overlap")]
[Category("BookingService")]
public async Task HasOverlapAsync_WhenOverlapExists_ReturnsTrue() { }
```

Dùng categories để lọc:
```bash
dotnet test --filter "Category=Overlap"
```

### 2.5. Test Data Seed Helper

```csharp
// Helpers/TestDataFactory.cs
public static class TestDataFactory
{
    public static Booking CreateApprovedBooking(string id, string roomId, int startHour, int endHour)
    {
        return new Booking
        {
            Id = id,
            RoomId = roomId,
            UserId = "USR-0001",
            Title = "Test",
            StartTime = new DateTime(2026, 7, 20, startHour, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 20, endHour, 0, 0, DateTimeKind.Utc),
            TotalPrice = 100_000,
            Status = BookingStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Room CreateRoom(string id, string name, int capacity, decimal pricePerHour = 100_000)
    {
        return new Room
        {
            Id = id,
            Name = name,
            Location = "Tầng 2",
            Capacity = capacity,
            PricePerHour = pricePerHour,
            IsActive = true
        };
    }

    public static Wallet CreateWallet(string userId, decimal balance)
    {
        return new Wallet { UserId = userId, Balance = balance };
    }
}
```

---

## 3. Unit Test cho Services

### 3.1. BookingService — Overlap Tests

```csharp
using Microsoft.EntityFrameworkCore;
using RoomBookingApp.Data;
using RoomBookingApp.Models;
using RoomBookingApp.Services;

namespace Co_working_Space.Nunit.Services;

[TestFixture]
public class BookingServiceTests
{
    private ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var db = new ApplicationDbContext(options);

        db.Rooms.Add(TestDataFactory.CreateRoom("RM-M-001", "Phòng A", 6));

        db.Bookings.Add(TestDataFactory.CreateApprovedBooking(
            "BKG-20260720-001", "RM-M-001", 9, 10));

        db.SaveChanges();
        return db;
    }

    // --- Overlap ---

    [Test]
    public async Task HasOverlapAsync_WhenOverlapExists_ReturnsTrue()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 10, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasOverlapAsync_WhenNoOverlap_ReturnsFalse()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_ExcludesCancelledBookings()
    {
        using var db = CreateDb();
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

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_WhenTimeRangeTouchesAtStart_ReturnsFalse()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        // Existing: 09:00-10:00, New: 10:00-11:00 → chạm nhau ở 10:00
        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_ExcludesDifferentRoom()
    {
        using var db = CreateDb();
        db.Rooms.Add(TestDataFactory.CreateRoom("RM-S-001", "Phòng B", 4));
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.HasOverlapAsync(
            "RM-S-001",
            new DateTime(2026, 7, 20, 9, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 10, 30, 0, DateTimeKind.Utc));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlapAsync_WhenUpdating_ExcludesOwnBooking()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        // Cập nhật booking: truyền currentBookingId = "BKG-20260720-001"
        var result = await service.HasOverlapAsync(
            "RM-M-001",
            new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            currentBookingId: "BKG-20260720-001");

        Assert.That(result, Is.False);
    }
}
```

### 3.2. BookingService — Create Tests

```csharp
[TestFixture]
public class BookingServiceCreateTests
{
    private ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var db = new ApplicationDbContext(options);

        db.Rooms.Add(TestDataFactory.CreateRoom("RM-M-001", "Phòng A", 6, 100_000));
        db.SaveChanges();
        return db;
    }

    [Test]
    public async Task CreateBookingAsync_ValidRequest_ReturnsBookingId()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var model = new CreateBookingViewModel
        {
            RoomId = "RM-M-001",
            Title = "Họp nhóm",
            StartTime = new DateTime(2026, 7, 25, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 25, 11, 0, 0, DateTimeKind.Utc)
        };

        var bookingId = await service.CreateBookingAsync(model, "USR-0001");

        Assert.That(bookingId, Is.Not.Null);
        Assert.That(bookingId, Does.StartWith("BKG-"));
    }

    [Test]
    public async Task CreateBookingAsync_EndTimeBeforeStart_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new BookingService(db);

        var model = new CreateBookingViewModel
        {
            RoomId = "RM-M-001",
            Title = "Test",
            StartTime = new DateTime(2026, 7, 25, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 25, 9, 0, 0, DateTimeKind.Utc)
        };

        var result = await service.CreateBookingAsync(model, "USR-0001");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateBookingAsync_Overlap_ReturnsNull()
    {
        using var db = CreateDb();
        // Add existing booking
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260725-001",
            RoomId = "RM-M-001",
            UserId = "USR-0001",
            Title = "Existing",
            StartTime = new DateTime(2026, 7, 25, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 25, 10, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Approved
        });
        db.SaveChanges();

        var service = new BookingService(db);

        var model = new CreateBookingViewModel
        {
            RoomId = "RM-M-001",
            Title = "Test",
            StartTime = new DateTime(2026, 7, 25, 9, 30, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 25, 10, 30, 0, DateTimeKind.Utc)
        };

        var result = await service.CreateBookingAsync(model, "USR-0002");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateBookingAsync_InactiveRoom_ReturnsNull()
    {
        using var db = CreateDb();
        var room = await db.Rooms.FindAsync("RM-M-001");
        room!.IsActive = false;
        await db.SaveChangesAsync();

        var service = new BookingService(db);

        var model = new CreateBookingViewModel
        {
            RoomId = "RM-M-001",
            Title = "Test",
            StartTime = new DateTime(2026, 7, 25, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 25, 10, 0, 0, DateTimeKind.Utc)
        };

        var result = await service.CreateBookingAsync(model, "USR-0001");

        Assert.That(result, Is.Null);
    }

    // ponytail: InMemory không test được Serializable transaction isolation.
    // Thêm integration test với SQL Server khi cần chứng minh race condition.
}
```

### 3.3. RoomService — Search Tests

```csharp
[TestFixture]
public class RoomServiceTests
{
    private ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var db = new ApplicationDbContext(options);

        db.Rooms.AddRange(
            TestDataFactory.CreateRoom("RM-S-001", "Phòng Nhỏ", 4),
            TestDataFactory.CreateRoom("RM-M-001", "Phòng Vừa", 8),
            TestDataFactory.CreateRoom("RM-L-001", "Phòng Lớn", 15),
            TestDataFactory.CreateRoom("RM-V-001", "Phòng VIP", 6) { IsActive = false }
        );
        db.SaveChanges();
        return db;
    }

    [Test]
    public async Task SearchAsync_NoFilter_ReturnsAllActiveRooms()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, null, null);

        Assert.That(rooms, Has.Count.EqualTo(3)); // VIP inactive
    }

    [Test]
    public async Task SearchAsync_MinCapacity_ReturnsMatchingRooms()
    {
        using var db = CreateDb();
        var service = new RoomService(db);

        var rooms = await service.SearchAsync(5, null, null);

        Assert.That(rooms, Has.All.Matches<Room>(r => r.Capacity >= 5));
    }

    [Test]
    public async Task SearchAsync_LocationFilter_ReturnsMatchingRooms()
    {
        using var db = CreateDb();
        var roomsList = await db.Rooms.ToListAsync();
        roomsList.ForEach(r => r.Location = "Tầng 2");
        await db.SaveChangesAsync();

        var service = new RoomService(db);

        var rooms = await service.SearchAsync(null, "Tầng 2", null);

        Assert.That(rooms, Is.Not.Empty);
    }
}
```

### 3.4. ApprovalService — Approve / Reject Tests

```csharp
[TestFixture]
public class ApprovalServiceTests
{
    private ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"Test_{Guid.NewGuid()}")
            .Options;
        var db = new ApplicationDbContext(options);

        db.Rooms.Add(TestDataFactory.CreateRoom("RM-M-001", "Phòng A", 6, 100_000));
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-001",
            RoomId = "RM-M-001",
            UserId = "USR-0001",
            Title = "Test",
            StartTime = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            TotalPrice = 100_000,
            Status = BookingStatus.Pending
        });
        db.Wallets.Add(TestDataFactory.CreateWallet("USR-0001", 500_000));
        db.SaveChanges();
        return db;
    }

    [Test]
    public async Task ApproveAsync_ValidBooking_DeductsWallet()
    {
        using var db = CreateDb();
        var service = new ApprovalService(db);

        var (success, error) = await service.ApproveAsync("BKG-20260720-001", "STF-0001");

        Assert.That(success, Is.True);
        Assert.That(error, Is.Null);

        var booking = await db.Bookings.FindAsync("BKG-20260720-001");
        Assert.That(booking!.Status, Is.EqualTo(BookingStatus.Approved));
        Assert.That(booking.PaymentStatus, Is.EqualTo(PaymentStatus.Paid));

        var wallet = await db.Wallets.FindAsync("USR-0001");
        Assert.That(wallet!.Balance, Is.EqualTo(400_000));
    }

    [Test]
    public async Task ApproveAsync_InsufficientBalance_ReturnsError()
    {
        using var db = CreateDb();
        var wallet = await db.Wallets.FindAsync("USR-0001");
        wallet!.Balance = 50_000;
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        var (success, error) = await service.ApproveAsync("BKG-20260720-001", "STF-0001");

        Assert.That(success, Is.False);
        Assert.That(error, Does.Contain("Số dư không đủ"));
    }

    [Test]
    public async Task RejectAsync_RefundsIfPaid()
    {
        using var db = CreateDb();
        var booking = await db.Bookings.FindAsync("BKG-20260720-001");
        booking!.PaymentStatus = PaymentStatus.Paid;
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        var result = await service.RejectAsync("BKG-20260720-001", "STF-0001", "Lịch bận");

        Assert.That(result, Is.True);

        var wallet = await db.Wallets.FindAsync("USR-0001");
        Assert.That(wallet!.Balance, Is.EqualTo(600_000)); // 500k + 100k refund

        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Rejected));
        Assert.That(booking.PaymentStatus, Is.EqualTo(PaymentStatus.Refunded));
    }

    [Test]
    public async Task GetPendingAsync_ReturnsOnlyPendingBookings()
    {
        using var db = CreateDb();
        // Add approved + cancelled
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-002",
            RoomId = "RM-M-001",
            UserId = "USR-0001",
            StartTime = new DateTime(2026, 7, 21, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Approved
        });
        db.Bookings.Add(new Booking
        {
            Id = "BKG-20260720-003",
            RoomId = "RM-M-001",
            UserId = "USR-0001",
            StartTime = new DateTime(2026, 7, 22, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc),
            Status = BookingStatus.Approved
        });
        await db.SaveChangesAsync();

        var service = new ApprovalService(db);

        var pending = await service.GetPendingAsync();

        Assert.That(pending, Has.Count.EqualTo(1));
        Assert.That(pending[0].Id, Is.EqualTo("BKG-20260720-001"));
    }
}
```

---

## 4. Unit Test cho Controllers

Dùng Moq để mock services + UserManager + SignInManager.

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Moq;
using RoomBookingApp.Controllers;
using RoomBookingApp.Models;

namespace Co_working_Space.Nunit.Controllers;

[TestFixture]
public class AccountControllerTests
{
    private Mock<UserManager<IdentityUser>> _userManager = null!;
    private Mock<SignInManager<IdentityUser>> _signInManager = null!;
    private AccountController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        var userStore = new Mock<IUserStore<IdentityUser>>();
        _userManager = new Mock<UserManager<IdentityUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _signInManager = new Mock<SignInManager<IdentityUser>>(
            _userManager.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

        _controller = new AccountController(_userManager.Object, _signInManager.Object);
    }

    [Test]
    public void Register_Get_ReturnsView()
    {
        var result = _controller.Register();
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Register_Post_ValidModel_CreatesUser()
    {
        _userManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _signInManager.Setup(x => x.SignInAsync(It.IsAny<IdentityUser>(), false, null))
            .Returns(Task.CompletedTask);

        var model = new RegisterViewModel
        {
            Email = "test@test.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123"
        };

        var result = await _controller.Register(model);

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        _userManager.Verify(x => x.CreateAsync(
            It.Is<IdentityUser>(u => u.Email == "test@test.com"), "Test@123"), Times.Once);
    }

    [Test]
    public async Task Register_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Email", "Required");

        var result = await _controller.Register(new RegisterViewModel());

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Login_Post_ValidCredentials_RedirectsToHome()
    {
        _signInManager.Setup(x => x.PasswordSignInAsync(
            "test@test.com", "Test@123", false, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var model = new LoginViewModel
        {
            Email = "test@test.com",
            Password = "Test@123",
            RememberMe = false
        };

        var result = await _controller.Login(model);

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }
}
```

### Controller Auth Tests

```csharp
[TestFixture]
public class BookingControllerAuthTests
{
    [Test]
    public void BookingController_HasAuthorizeAttribute()
    {
        var attr = typeof(BookingController).GetCustomAttributes(
            typeof(AuthorizeAttribute), false);

        Assert.That(attr, Is.Not.Empty);
    }
}

[TestFixture]
public class AdminControllerAuthTests
{
    [TestCase("Admin/RoomController", "Admin,Staff")]
    [TestCase("Admin/EquipmentController", "Admin,Staff")]
    [TestCase("Admin/BookingController", "Admin,Staff")]
    [TestCase("Admin/WalletController", "Admin,Staff")]
    [TestCase("Admin/UserController", "Admin,Staff")]
    [TestCase("Admin/DashboardController", "Admin")]
    public void AdminController_HasCorrectRoles(string controllerName, string expectedRoles)
    {
        var type = typeof(RoomBookingApp.Areas.Admin.Controllers.RoomController).Assembly
            .GetTypes()
            .FirstOrDefault(t => t.FullName!.Contains(controllerName.Replace('/', '.')));

        Assert.That(type, Is.Not.Null, $"Controller {controllerName} not found");

        var authorizeAttr = type!.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        Assert.That(authorizeAttr, Is.Not.Null, $"{controllerName} missing [Authorize]");
        Assert.That(authorizeAttr.Roles, Is.EqualTo(expectedRoles));
    }

    [TestCase("RoomController", "Create")]
    [TestCase("RoomController", "Edit")]
    [TestCase("RoomController", "ManageEquipment")]
    public void AdminAction_HasAdminOnlyRole(string controllerName, string actionName)
    {
        var type = typeof(RoomBookingApp.Areas.Admin.Controllers.RoomController).Assembly
            .GetTypes()
            .FirstOrDefault(t => t.Name == controllerName);

        var method = type!.GetMethod(actionName);
        var attr = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().FirstOrDefault();

        Assert.That(attr, Is.Not.Null);
        Assert.That(attr.Roles, Is.EqualTo("Admin"));
    }
}
```

---

## 5. IdGenerator Tests

```csharp
namespace Co_working_Space.Nunit.Helpers;

[TestFixture]
public class IdGeneratorTests
{
    [Test]
    public void Next_BookingPrefix_IncludesDate()
    {
        var id = IdGenerator.Next(IdGenerator.Booking);
        Assert.That(id, Does.Match(@"^BKG-\d{8}-\d{3}$"));
    }

    [Test]
    public void Next_UserPrefix_ReturnsSequential()
    {
        var id1 = IdGenerator.Next(IdGenerator.User);
        var id2 = IdGenerator.Next(IdGenerator.User);

        Assert.That(id2, Is.GreaterThan(id1));
    }

    [Test]
    public void Next_DifferentPrefixes_HaveDifferentCounters()
    {
        var userId = IdGenerator.Next(IdGenerator.User);
        var roomId = IdGenerator.Next(IdGenerator.RoomSmall);

        Assert.That(userId, Does.StartWith("USR-"));
        Assert.That(roomId, Does.StartWith("RM-S-"));
    }
}
```

---

## 6. Ma trận Test Case → Unit Test

### 6.1. User Flow

| TC# | Mô tả | Loại | File Test |
|-----|-------|------|-----------|
| TC1 | Đăng ký thành công | Unit | `AccountControllerTests.cs` |
| TC2 | Đăng nhập/Đăng xuất | Unit | `AccountControllerTests.cs` |
| TC3 | Phân quyền User | Unit | `AdminControllerAuthTests.cs` |
| TC4–5 | Lọc phòng | Unit | `RoomServiceTests.cs` |
| TC6 | Đặt phòng thành công | Unit | `BookingServiceCreateTests.cs` |
| TC7 | Đặt phòng trùng lịch | Unit | `BookingServiceCreateTests.cs` |
| TC8 | Giờ trong quá khứ | Unit | `BookingServiceCreateTests.cs` |
| TC9 | TotalPrice tự động tính | Unit | `BookingServiceCreateTests.cs` |
| TC10 | Over-posting bị chặn | Unit | ViewModel test (dùng `TryValidateModel`) |
| TC11 | Race condition | Integration | Bỏ qua unit test — cần SQL Server thật |
| TC12 | Lịch sử đặt phòng | Unit | `BookingControllerTests.cs` |
| TC13–14 | Hủy đơn | Unit | `BookingControllerTests.cs` |

### 6.2. Admin/Staff Flow

| TC# | Mô tả | Loại | File Test |
|-----|-------|------|-----------|
| TC15 | Admin tạo phòng | Unit | `AdminRoomControllerTests.cs` |
| TC16 | Admin sửa phòng | Unit | `AdminRoomControllerTests.cs` |
| TC17 | Bảo trì phòng | Unit | `AdminRoomControllerTests.cs` |
| TC18–19 | CRUD thiết bị | Unit | `AdminEquipmentControllerTests.cs` |
| TC20–23 | Duyệt/Từ chối đơn | Unit | `ApprovalServiceTests.cs` |
| TC24 | Gán thiết bị cho phòng | Unit | `AdminRoomControllerTests.cs` |
| TC25–27 | Staff quản lý thiết bị | Unit | `AdminEquipmentControllerTests.cs` |
| TC28–31 | Admin/Staff phân quyền thiết bị | Unit | `AdminControllerAuthTests.cs` |
| TC32 | Dashboard thống kê | Unit | `AdminDashboardControllerTests.cs` |
| TC33–35 | Staff quản lý phòng | Unit | `AdminRoomControllerTests.cs` |
| TC36–40 | Quản lý user + reset password | Unit | `AdminUserControllerTests.cs` |
| TC41–45 | Ví tiền + Nạp + Trừ | Unit | `ApprovalServiceTests.cs` + `AdminWalletControllerTests.cs` |
| TC46–49 | Upload ảnh | Unit | `AdminRoomControllerTests.cs` |

### 6.3. System

| TC# | Mô tả | Loại | File Test |
|-----|-------|------|-----------|
| TC50 | Seed Admin | Unit | Seed test hoặc Integration |
| TC51 | Seed Role | Unit | Seed test |
| TC52 | FK cascade | Integration | Bỏ qua unit |
| TC53 | Index overlap | Integration | Bỏ qua unit |

---

## 7. Chạy Test

```bash
# Chạy tất cả
dotnet test

# Chạy theo category
dotnet test --filter "Category=BookingService"

# Chạy theo tên
dotnet test --filter "FullyQualifiedName~Overlap"

# Với coverage
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

---

## 8. Checklist Test

### Services
- [ ] `BookingServiceTests.cs` — HasOverlapAsync (5+ test cases)
- [ ] `BookingServiceCreateTests.cs` — CreateBookingAsync (5+ test cases)
- [ ] `RoomServiceTests.cs` — SearchAsync (3+ test cases)
- [ ] `ApprovalServiceTests.cs` — ApproveAsync, RejectAsync, GetPendingAsync (5+ test cases)

### Controllers — User
- [ ] `AccountControllerTests.cs` — Register, Login, Logout, Profile
- [ ] `BookingControllerTests.cs` — Create, MyBookings, Cancel
- [ ] `RoomControllerTests.cs` — Index with filters

### Controllers — Admin
- [ ] `AdminRoomControllerTests.cs` — Index, Create, Edit, ToggleStatus, ManageEquipment
- [ ] `AdminEquipmentControllerTests.cs` — Index, Create, Delete, UpdateStatus, Transfer
- [ ] `AdminBookingControllerTests.cs` — Pending, Approve, Reject
- [ ] `AdminWalletControllerTests.cs` — Index, TopUp
- [ ] `AdminUserControllerTests.cs` — Index, ResetPassword
- [ ] `AdminDashboardControllerTests.cs` — Index

### Security & Auth
- [ ] `AdminControllerAuthTests.cs` — Route matrix enforcement
- [ ] `BookingControllerAuthTests.cs` — [Authorize] check
- [ ] `AdminActionAuthTests.cs` — Admin-only actions

### Helpers
- [ ] `IdGeneratorTests.cs` — prefix, date, sequential
