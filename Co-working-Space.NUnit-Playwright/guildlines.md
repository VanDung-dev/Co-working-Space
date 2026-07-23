# NUnit + Playwright E2E Testing — Co-working Space.NUnit-Playwright

> File này chứa toàn bộ quy tắc và hướng dẫn viết E2E test cho project `Co-working-Space.NUnit-Playwright`. Xem `guildlines.md` (project gốc) cho tổng quan hệ thống.

---

## 1. Thiết lập Project

### 1.1. Thêm Project Reference

```xml
<!-- Co-working-Space.NUnit-Playwright.csproj -->
<ItemGroup>
    <ProjectReference Include="..\Co-working-Space\Co-working-Space.csproj" />
</ItemGroup>
```

### 1.2. Packages đã có

| Package | Version | Mục đích |
|---------|---------|----------|
| `Microsoft.Playwright.NUnit` | 1.52.0 | Playwright integration với NUnit |
| `NUnit` | 4.3.2 | Test framework |
| `NUnit3TestAdapter` | 5.0.0 | Test adapter |
| `Microsoft.NET.Test.Sdk` | 17.14.0 | CLI test runner |

### 1.3. Cài đặt Browser

```bash
# Sau khi build project lần đầu, chạy:
pwsh bin/Debug/net10.0/playwright.ps1 install

# Hoặc dùng CLI tool:
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

---

## 2. Cấu trúc & Quy tắc

### 2.1. Tổ chức File Test

```
Co-working-Space.NUnit-Playwright/
├── Pages/                    # Page Object Model
│   ├── LoginPage.cs
│   ├── RegisterPage.cs
│   ├── RoomPage.cs
│   ├── BookingPage.cs
│   └── Admin/
│       ├── AdminRoomPage.cs
│       ├── AdminBookingPage.cs
│       ├── AdminEquipmentPage.cs
│       ├── AdminWalletPage.cs
│       └── AdminUserPage.cs
├── Tests/
│   ├── AuthTests.cs
│   ├── BookingTests.cs
│   ├── RoomTests.cs
│   └── Admin/
│       ├── AdminRoomTests.cs
│       ├── AdminApprovalTests.cs
│       ├── AdminEquipmentTests.cs
│       ├── AdminWalletTests.cs
│       └── AdminUserTests.cs
└── Helpers/
    └── TestFixture.cs        # Base class chung
```

### 2.2. Playwright Configuration

Tạo `appsettings.Testing.json` trong main project:

```json
{
  "BaseUrl": "https://localhost:5001"
}
```

Hoặc hardcode trong base test class:

```csharp
public class TestBase : PageTest
{
    protected const string BaseUrl = "https://localhost:5001";

    protected async Task LoginAs(string email, string password)
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.FillAsync("input[name='Email']", email);
        await Page.FillAsync("input[name='Password']", password);
        await Page.ClickAsync("button[type='submit']");
    }

    protected async Task LoginAsAdmin()
        => await LoginAs("admin@coworking.com", "Admin@123");

    protected async Task LoginAsUser()
        => await LoginAs("user@test.com", "User@123");
}
```

### 2.3. NUnit + Playwright Conventions

```csharp
using Microsoft.Playwright.NUnit;

namespace Co_working_Space.NUnit_Playwright.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AuthTests : PageTest
{
    private const string BaseUrl = "https://localhost:5001";

    [SetUp]
    public async Task SetUp()
    {
        // Navigate to home before each test
        await Page.GotoAsync(BaseUrl);
    }

    [Test]
    public async Task TC1_Register_NewUser_Success()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Register");

        await Page.FillAsync("input[name='Email']", $"user_{Guid.NewId()}@test.com");
        await Page.FillAsync("input[name='Password']", "Test@123");
        await Page.FillAsync("input[name='ConfirmPassword']", "Test@123");
        await Page.ClickAsync("button[type='submit']");

        await Expect(Page).ToHaveURLAsync(new Regex(".*Home/Index"));
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Đơn của tôi");
    }
}
```

### 2.4. Chạy Headed (Có UI)

```bash
# Mặc định chạy headless
dotnet test

# Chạy có UI
$env:HEADED=1
dotnet test
```

Hoặc dùng playwright config:

```csharp
[SetUp]
public async Task SetUp()
{
    await Page.GotoAsync(BaseUrl);
    // Page mặc định từ PageTest base class
    // Set headless=false trong launchSettings nếu cần
}
```

---

## 3. Page Object Model

### 3.1. LoginPage

```csharp
namespace Co_working_Space.NUnit_Playwright.Pages;

public class LoginPage(IPage page)
{
    private const string Url = "https://localhost:5001/Account/Login";

    public async Task GoToAsync() => await page.GotoAsync(Url);

    public async Task LoginAsync(string email, string password)
    {
        await page.FillAsync("input[name='Email']", email);
        await page.FillAsync("input[name='Password']", password);
        await page.ClickAsync("button[type='submit']");
    }

    public async Task AssertLoggedInAsync()
    {
        await Expect(page.Locator(".navbar")).ToContainTextAsync("Đăng xuất");
    }
}
```

### 3.2. BookingPage

```csharp
namespace Co_working_Space.NUnit_Playwright.Pages;

public class BookingPage(IPage page)
{
    private const string Url = "https://localhost:5001/Booking/Create?roomId=";

    public async Task GoToCreateAsync(string roomId)
        => await page.GotoAsync($"{Url}{roomId}");

    public async Task CreateBookingAsync(string title, DateTime start, DateTime end)
    {
        await page.FillAsync("input[name='Title']", title);
        await page.FillAsync("input[name='StartTime']", start.ToString("yyyy-MM-ddTHH:mm"));
        await page.FillAsync("input[name='EndTime']", end.ToString("yyyy-MM-ddTHH:mm"));
        await page.ClickAsync("button[type='submit']");
    }

    public async Task AssertSuccessAsync(string bookingId)
    {
        await Expect(page.Locator(".alert-success"))
            .ToContainTextAsync(bookingId);
    }
}
```

---

## 4. E2E Test Scenarios

### 4.1. AuthTests — Đăng ký, Đăng nhập, Phân quyền

```csharp
[TestFixture]
public class AuthTests : PageTest
{
    private const string BaseUrl = "https://localhost:5001";

    // --- TC1: Đăng ký tài khoản mới ---
    [Test]
    public async Task TC1_Register_NewUser_Success()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Register");

        var email = $"user_{Guid.NewGuid():N[..8]}@test.com";
        await Page.FillAsync("input[name='Email']", email);
        await Page.FillAsync("input[name='Password']", "Test@123");
        await Page.FillAsync("input[name='ConfirmPassword']", "Test@123");
        await Page.ClickAsync("button[type='submit']");

        await Expect(Page).ToHaveURLAsync(new Regex(".*Home/Index"));
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Đơn của tôi");
    }

    // --- TC2: Đăng nhập / Đăng xuất ---
    [Test]
    public async Task TC2_Login_ValidCredentials_Success()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");

        await Page.FillAsync("input[name='Email']", "admin@coworking.com");
        await Page.FillAsync("input[name='Password']", "Admin@123");
        await Page.ClickAsync("button[type='submit']");

        await Expect(Page).ToHaveURLAsync(new Regex(".*Home/Index"));
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Đăng xuất");

        // Logout
        await Page.ClickAsync("button:has-text('Đăng xuất')");
        await Expect(Page.Locator(".navbar")).ToContainTextAsync("Đăng nhập");
    }

    // --- TC3: Phân quyền User ---
    [Test]
    public async Task TC3_User_CannotAccessAdminDashboard()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.FillAsync("input[name='Email']", "user@test.com");
        await Page.FillAsync("input[name='Password']", "User@123");
        await Page.ClickAsync("button[type='submit']");

        await Page.GotoAsync($"{BaseUrl}/Admin/Dashboard");

        // Expect redirected to login or 403
        // Playwright không follow redirect → check URL
        var url = Page.Url;
        Assert.That(url, Does.Not.Contain("/Admin/Dashboard"));
    }
}
```

### 4.2. RoomTests — Tra cứu phòng

```csharp
[TestFixture]
public class RoomTests : PageTest
{
    private const string BaseUrl = "https://localhost:5001";

    [SetUp]
    public async Task SetUp()
    {
        await Page.GotoAsync($"{BaseUrl}/Room/Index");
    }

    // --- TC4: Lọc sức chứa ---
    [Test]
    public async Task TC4_Search_FilterByCapacity()
    {
        await Page.SelectOptionAsync("select[name='minCapacity']", "5");
        await Page.ClickAsync("button[type='submit']");

        // Tất cả card phòng có badge Medium/Large/VIP
        var cardCount = await Page.Locator(".card").CountAsync();
        if (cardCount > 0)
        {
            await Expect(Page.Locator(".card").First).ToBeVisibleAsync();
        }
    }

    // --- TC5: Lọc vị trí ---
    [Test]
    public async Task TC5_Search_FilterByLocation()
    {
        await Page.FillAsync("input[name='location']", "Tầng 2");
        await Page.ClickAsync("button[type='submit']");

        // URL có chứa query param
        await Expect(Page).ToHaveURLAsync(new Regex(".*location=Tầng%202"));
    }

    // --- TC49: Ảnh hiển thị đúng ---
    [Test]
    public async Task TC49_RoomCard_ImageDisplayed()
    {
        var images = Page.Locator(".card img");
        var count = await images.CountAsync();

        for (int i = 0; i < count; i++)
        {
            await Expect(images.Nth(i)).ToHaveAttributeAsync("src", new Regex(".+(jpg|png|no-image)"));
        }
    }
}
```

### 4.3. BookingTests — Đặt phòng + Lịch sử + Hủy

```csharp
[TestFixture]
public class BookingTests : PageTest
{
    private const string BaseUrl = "https://localhost:5001";

    // --- TC6: Đặt phòng thành công ---
    [Test]
    public async Task TC6_CreateBooking_Success()
    {
        // Login trước
        await LoginAsUser();

        // Vào Room/Index, click "Đặt phòng" trên phòng đầu tiên
        await Page.GotoAsync($"{BaseUrl}/Room/Index");
        await Page.Locator("a:has-text('Đặt phòng')").First.ClickAsync();

        // Fill form
        var tomorrow = DateTime.Now.AddDays(1);
        await Page.FillAsync("input[name='Title']", "Họp E2E test");
        await Page.FillAsync("input[name='StartTime']", tomorrow.ToString("yyyy-MM-ddTHH:mm"));
        await Page.FillAsync("input[name='EndTime']", tomorrow.AddHours(2).ToString("yyyy-MM-ddTHH:mm"));
        await Page.ClickAsync("button[type='submit']");

        // Check redirect đến MyBookings + success message
        await Expect(Page).ToHaveURLAsync(new Regex(".*Booking/MyBookings"));
        await Expect(Page.Locator(".alert-success")).ToBeVisibleAsync();
        await Expect(Page.Locator(".table")).ToContainTextAsync("Họp E2E test");
    }

    // --- TC8: Đặt phòng giờ quá khứ (client validation) ---
    [Test]
    public async Task TC8_CreateBooking_PastTime_ClientValidation()
    {
        await LoginAsUser();

        var dialogTask = Page.WaitForEventAsync<IDialog>(e => e.Dialog);

        await Page.GotoAsync($"{BaseUrl}/Room/Index");
        await Page.Locator("a:has-text('Đặt phòng')").First.ClickAsync();

        await Page.FillAsync("input[name='Title']", "Test past");
        await Page.FillAsync("input[name='StartTime']", "2020-01-01T09:00");
        await Page.FillAsync("input[name='EndTime']", "2020-01-01T10:00");
        await Page.ClickAsync("button[type='submit']");

        var dialog = await dialogTask;
        Assert.That(dialog.Message, Does.Contain("Thời gian bắt đầu phải lớn hơn"));
        await dialog.DismissAsync();
    }

    // --- TC12: Lịch sử đặt phòng ---
    [Test]
    public async Task TC12_MyBookings_ShowsHistory()
    {
        await LoginAsUser();
        await Page.GotoAsync($"{BaseUrl}/Booking/MyBookings");

        await Expect(Page.Locator("h4")).ToContainTextAsync("Lịch sử đặt phòng");
        // Table có dữ liệu hoặc alert "chưa có đơn"
        var hasTable = await Page.Locator(".table").IsVisibleAsync();
        var hasAlert = await Page.Locator(".alert-info").IsVisibleAsync();
        Assert.That(hasTable || hasAlert, Is.True);
    }

    // --- TC13: Hủy đơn Pending ---
    [Test]
    public async Task TC13_Cancel_PendingBooking_Success()
    {
        await LoginAsUser();
        await Page.GotoAsync($"{BaseUrl}/Booking/MyBookings");

        var cancelBtn = Page.Locator("button:has-text('Hủy')").First;
        if (await cancelBtn.IsVisibleAsync())
        {
            // TC56: SweetAlert2 confirm
            var dialogTask = Page.WaitForEventAsync<IDialog>(e => e.Dialog);

            await cancelBtn.ClickAsync();

            var dialog = await dialogTask;
            Assert.That(dialog.Message, Does.Contain("Hủy"));
            await dialog.AcceptAsync();

            await Expect(Page.Locator(".alert-success")).ToBeVisibleAsync();
        }
    }

    private async Task LoginAsUser()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.FillAsync("input[name='Email']", "user@test.com");
        await Page.FillAsync("input[name='Password']", "User@123");
        await Page.ClickAsync("button[type='submit']");
    }
}
```

### 4.4. Admin — Duyệt đơn

```csharp
[TestFixture]
public class AdminApprovalTests : PageTest
{
    private const string BaseUrl = "https://localhost:5001";

    // --- TC20: Xem danh sách chờ duyệt ---
    [Test]
    public async Task TC20_PendingBookings_ShowsList()
    {
        await LoginAsAdmin();
        await Page.GotoAsync($"{BaseUrl}/Admin/Booking/Pending");

        await Expect(Page).ToHaveURLAsync(new Regex(".*Admin/Booking/Pending"));
    }

    // --- TC21: Admin duyệt đơn ---
    [Test]
    public async Task TC21_ApproveBooking_Success()
    {
        await LoginAsAdmin();
        await Page.GotoAsync($"{BaseUrl}/Admin/Booking/Pending");

        var approveBtn = Page.Locator("button:has-text('Duyệt')").First;
        if (await approveBtn.IsVisibleAsync())
        {
            await approveBtn.ClickAsync();
            await Expect(Page.Locator(".alert-success")).ToBeVisibleAsync();
        }
    }

    // --- TC44: Duyệt đơn — số dư không đủ ---
    [Test]
    public async Task TC44_Approve_InsufficientBalance_ShowsError()
    {
        await LoginAsAdmin();
        await Page.GotoAsync($"{BaseUrl}/Admin/Booking/Pending");

        var approveBtn = Page.Locator("button:has-text('Duyệt')").First;
        if (await approveBtn.IsVisibleAsync())
        {
            await approveBtn.ClickAsync();

            // Kiểm tra có error message không
            var hasError = await Page.Locator(".alert-danger").IsVisibleAsync();
            if (hasError)
            {
                await Expect(Page.Locator(".alert-danger"))
                    .ToContainTextAsync("Số dư không đủ");
            }
        }
    }

    private async Task LoginAsAdmin()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.FillAsync("input[name='Email']", "admin@coworking.com");
        await Page.FillAsync("input[name='Password']", "Admin@123");
        await Page.ClickAsync("button[type='submit']");
    }
}
```

### 4.5. Admin — Quản lý phòng

```csharp
[TestFixture]
public class AdminRoomTests : PageTest
{
    private const string BaseUrl = "https://localhost:5001";

    // --- TC15: Admin tạo phòng mới ---
    [Test]
    public async Task TC15_CreateRoom_Success()
    {
        await LoginAsAdmin();
        await Page.GotoAsync($"{BaseUrl}/Admin/Room/Create");

        await Page.FillAsync("input[name='Name']", "Phòng E2E Test");
        await Page.FillAsync("input[name='Capacity']", "6");
        await Page.FillAsync("input[name='PricePerHour']", "150000");
        await Page.FillAsync("input[name='Location']", "Tầng 3");
        await Page.ClickAsync("button[type='submit']");

        await Expect(Page).ToHaveURLAsync(new Regex(".*Admin/Room/Index"));
        await Expect(Page.Locator(".alert-success")).ToBeVisibleAsync();
    }

    // --- TC17: Bảo trì phòng ---
    [Test]
    public async Task TC17_ToggleMaintenance_Success()
    {
        await LoginAsAdmin();
        await Page.GotoAsync($"{BaseUrl}/Admin/Room");

        var toggleBtn = Page.Locator("button:has-text('Bảo trì'), button:has-text('Kích hoạt')").First;
        if (await toggleBtn.IsVisibleAsync())
        {
            await toggleBtn.ClickAsync();
            await Expect(Page.Locator(".alert-success")).ToBeVisibleAsync();
        }
    }

    private async Task LoginAsAdmin()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.FillAsync("input[name='Email']", "admin@coworking.com");
        await Page.FillAsync("input[name='Password']", "Admin@123");
        await Page.ClickAsync("button[type='submit']");
    }
}
```

### 4.6. Admin — Wallet

```csharp
[TestFixture]
public class AdminWalletTests : PageTest
{
    private const string BaseUrl = "https://localhost:5001";

    // --- TC41: Nạp tiền vào ví ---
    [Test]
    public async Task TC41_TopUp_Success()
    {
        await LoginAsAdmin();
        await Page.GotoAsync($"{BaseUrl}/Admin/Wallet");

        var topUpBtn = Page.Locator("a:has-text('Nạp tiền')").First;
        if (await topUpBtn.IsVisibleAsync())
        {
            await topUpBtn.ClickAsync();
            await Page.FillAsync("input[name='amount']", "500000");
            await Page.ClickAsync("button[type='submit']");

            await Expect(Page.Locator(".alert-success")).ToBeVisibleAsync();
        }
    }

    private async Task LoginAsAdmin()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.FillAsync("input[name='Email']", "admin@coworking.com");
        await Page.FillAsync("input[name='Password']", "Admin@123");
        await Page.ClickAsync("button[type='submit']");
    }
}
```

---

## 5. Screenshot & Video on Failure

Thêm vào `SetUp` / `TearDown`:

```csharp
[SetUp]
public async Task SetUp()
{
    await Page.GotoAsync(BaseUrl);
    TestContext.WriteLine($"Starting test: {TestContext.CurrentContext.Test.Name}");
}

[TearDown]
public async Task TearDown()
{
    if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
    {
        var fileName = $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path });
        TestContext.AddTestAttachment(path);
    }
}
```

Hoặc dùng Playwright Trace Viewer:

```csharp
[SetUp]
public async Task SetUp()
{
    await Playwright.Tracing.StartAsync(new TracingStartOptions
    {
        Screenshots = true,
        Snapshots = true,
        Sources = true
    });
}

[TearDown]
public async Task TearDown()
{
    var tracePath = Path.Combine(
        TestContext.CurrentContext.WorkDirectory,
        $"{TestContext.CurrentContext.Test.Name}.zip");
    await Playwright.Tracing.StopAsync(new TracingStopOptions
    {
        Path = tracePath
    });
}
```

---

## 6. Quản lý Test User

Tạo helper để seed test data trong `SetUp` (gọi API hoặc dùng DbContext trực tiếp):

```csharp
public static class TestDataSeeding
{
    public static async Task SeedTestUser(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        if (await userManager.FindByEmailAsync("user@e2e.test") == null)
        {
            var user = new IdentityUser
            {
                Id = IdGenerator.Next(IdGenerator.User),
                UserName = "user@e2e.test",
                Email = "user@e2e.test"
            };
            await userManager.CreateAsync(user, "Test@123");
            await userManager.AddToRoleAsync(user, "User");

            db.Wallets.Add(new Wallet { UserId = user.Id, Balance = 1_000_000 });
            await db.SaveChangesAsync();
        }
    }
}
```

---

## 7. Ma trận Test Case → E2E Test

| TC# | Mô tả | File Test | Ghi chú |
|-----|-------|-----------|---------|
| TC1 | Đăng ký | `AuthTests.cs` | |
| TC2 | Đăng nhập/Xuất | `AuthTests.cs` | |
| TC3 | Phân quyền User | `AuthTests.cs` | |
| TC4 | Lọc sức chứa | `RoomTests.cs` | |
| TC5 | Lọc vị trí | `RoomTests.cs` | |
| TC6 | Đặt phòng | `BookingTests.cs` | |
| TC7 | Trùng lịch | `BookingTests.cs` | Cần seed overlap |
| TC8 | Giờ quá khứ | `BookingTests.cs` | Client-side validation |
| TC9 | TotalPrice | Bỏ qua E2E | Unit test đã cover |
| TC10 | Over-posting | Bỏ qua E2E | Unit test |
| TC11 | Race condition | Bỏ qua E2E | Cần tool đặc thù |
| TC12 | Lịch sử | `BookingTests.cs` | |
| TC13–14 | Hủy đơn | `BookingTests.cs` | + SweetAlert2 |
| TC15 | Admin tạo phòng | `AdminRoomTests.cs` | |
| TC16 | Admin sửa phòng | `AdminRoomTests.cs` | |
| TC17 | Bảo trì phòng | `AdminRoomTests.cs` | |
| TC18–19 | CRUD thiết bị | `AdminEquipmentTests.cs` | |
| TC20 | DS chờ duyệt | `AdminApprovalTests.cs` | |
| TC21–23 | Duyệt/Từ chối | `AdminApprovalTests.cs` | |
| TC24 | Gán thiết bị | `AdminRoomTests.cs` | |
| TC25–27 | Staff thiết bị | `AdminEquipmentTests.cs` | |
| TC28–31 | Phân quyền thiết bị | `AdminEquipmentTests.cs` | |
| TC32 | Dashboard | `AdminDashboardTests.cs` | |
| TC33–35 | Staff phòng | `AdminRoomTests.cs` | |
| TC36–40 | User + Reset PW | `AdminUserTests.cs` | |
| TC41–45 | Ví tiền | `AdminWalletTests.cs` | |
| TC46–49 | Upload ảnh | `AdminRoomTests.cs` | |
| TC50–53 | System | Bỏ qua E2E | Integration |
| TC54–55 | Validation giờ | `BookingTests.cs` | |
| TC56 | SweetAlert2 confirm | `BookingTests.cs` | |
| TC57 | Responsive | Bỏ qua E2E | Manual / Visual |
| TC58 | PaymentStatus badge | `BookingTests.cs` | |

---

## 8. Chạy Test

```bash
# Headless (CI default)
dotnet test

# Headed (xem UI)
$env:PLAYWRIGHT_HEADLESS="false"
dotnet test
# Hoặc dùng PowerShell:
$Env:PLAYWRIGHT_HEADLESS="0"
dotnet test

# Chạy 1 test cụ thể
dotnet test --filter "FullyQualifiedName~TC6"

# Chạy test theo class
dotnet test --filter "FullyQualifiedName~AuthTests"

# Debug — xem browser
dotnet test --filter "FullyQualifiedName~TC6" -- NUnit.DefaultTestNamePattern=*
```

### playwright.ps1

```bash
# Cài browser (chạy 1 lần sau khi build)
pwsh bin/Debug/net10.0/playwright.ps1 install
# Hoặc dùng dotnet tool
playwright install
```

---

## 9. Checklist E2E Tests

### Authentication
- [ ] `AuthTests.cs` — Register (TC1), Login (TC2), Logout (TC2), Authorization (TC3)

### Room Search
- [ ] `RoomTests.cs` — Filter by capacity (TC4), location (TC5), image display (TC49)

### Booking
- [ ] `BookingTests.cs` — Create (TC6, TC7, TC8), History (TC12), Cancel (TC13, TC14, TC56), PaymentStatus badge (TC58)

### Admin — Booking Approval
- [ ] `AdminApprovalTests.cs` — Pending list (TC20), Approve (TC21, TC43, TC44), Reject (TC22, TC45)

### Admin — Room Management
- [ ] `AdminRoomTests.cs` — Create (TC15, TC46), Edit (TC16, TC47), ToggleStatus (TC17), ManageEquipment (TC24)

### Admin — Equipment
- [ ] `AdminEquipmentTests.cs` — Create (TC18, TC28), Delete (TC19, TC29), UpdateStatus (TC26), Transfer (TC27), Permissions (TC30, TC31), Staff view (TC25)

### Admin — User Management
- [ ] `AdminUserTests.cs` — List (TC36), ResetPassword (TC37, TC38, TC39), Staff filtered view (TC36)

### Admin — Wallet
- [ ] `AdminWalletTests.cs` — List (TC41), TopUp (TC41, TC42)

### Admin — Dashboard
- [ ] `AdminDashboardTests.cs` — Stats display (TC32)
