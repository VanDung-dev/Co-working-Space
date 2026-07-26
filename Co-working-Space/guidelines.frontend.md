# Frontend Implementation (Razor View & JS)

> File này chứa code mẫu giao diện người dùng sau khi tối ưu. Xem `guildlines.md` cho tổng quan hệ thống.

## Design Tokens (dùng trong `site.css` hoặc `<style>`)

Dùng CSS variables — đổi màu toàn bộ bằng 7 dòng, ko cần sửa từng view.

```css
:root {
  --primary: #2563eb;
  --primary-light: #dbeafe;
  --success: #16a34a;
  --warning: #d97706;
  --danger: #dc2626;
  --gray-50: #f9fafb;
  --gray-100: #f3f4f6;
  --gray-200: #e5e7eb;
  --gray-500: #6b7280;
  --gray-700: #374151;
  --gray-900: #111827;
  --radius: 8px;
}
```

## 1. _Layout.cshtml — Render TempData + SweetAlert2 (`Views/Shared/_Layout.cshtml`)

```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] — Co-working Space</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="~/css/site.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body class="d-flex flex-column min-vh-100">
    <nav class="navbar navbar-expand-lg navbar-dark" style="background: var(--gray-900);">
        <div class="container">
            <a class="navbar-brand fw-bold" href="/">Co-working Space</a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav ms-auto">
                    @if (User.Identity!.IsAuthenticated)
                    {
                        <li class="nav-item"><a class="nav-link" asp-controller="Booking" asp-action="MyBookings">Đơn của tôi</a></li>
                        <li class="nav-item"><a class="nav-link" asp-controller="Account" asp-action="Profile">Hồ sơ</a></li>
                        <li class="nav-item">
                            <form asp-controller="Account" asp-action="Logout" method="post" class="d-inline">
                                @Html.AntiForgeryToken()
                                <button type="submit" class="btn nav-link">Đăng xuất</button>
                            </form>
                        </li>
                    }
                    else
                    {
                        <li class="nav-item"><a class="nav-link" asp-controller="Account" asp-action="Register">Đăng ký</a></li>
                        <li class="nav-item"><a class="nav-link" asp-controller="Account" asp-action="Login">Đăng nhập</a></li>
                    }
                </ul>
            </div>
        </div>
    </nav>

    <main class="container py-4 flex-grow-1">
        @RenderBody()
    </main>

    <footer class="border-top py-3 text-center text-muted small">
        &copy; @DateTime.Now.Year — Co-working Space
    </footer>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)

    <script>
        // SweetAlert2 cho TempData — thay thế alert bootstrap cũ
        const success = '@TempData["SuccessMessage"]';
        const error = '@TempData["ErrorMessage"]';
        if (success) Swal.fire({ icon: 'success', title: success, timer: 3000, showConfirmButton: false });
        if (error) Swal.fire({ icon: 'error', title: error, timer: 5000, showConfirmButton: false });
    </script>
</body>
</html>
```

## 2. _Layout Admin (`Areas/Admin/Views/Shared/_Layout.cshtml`)

Sidebar + content — dùng flex, ko cần JS để toggle (dùng Bootstrap collapse).

```html
@{
    var area = "Admin";
    var controller = ViewContext.RouteData.Values["Controller"]?.ToString();
}

<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] — Admin</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="~/css/site.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body>
    <div class="d-flex" style="min-height: 100vh;">
        <aside class="bg-dark text-white" style="width: 240px; flex-shrink: 0;">
            <div class="p-3 fw-bold border-bottom border-secondary">
                <a href="/Admin/Dashboard" class="text-white text-decoration-none">Admin Panel</a>
            </div>
            <nav class="d-flex flex-column p-2 gap-1">
                @foreach (var item in new[] {
                    ("Dashboard", "Dashboard", "Index"),
                    ("Phòng họp", "Room", "Index"),
                    ("Thiết bị", "Equipment", "Index"),
                    ("Duyệt đơn", "Booking", "Pending"),
                    ("Người dùng", "User", "Index"),
                    ("Ví tiền", "Wallet", "Index"),
                })
                {
                    var (label, ctrl, act) = item;
                    var active = controller == ctrl;
                    <a asp-area="Admin" asp-controller="@ctrl" asp-action="@act"
                       class="btn btn-sm text-start @(active ? "btn-primary" : "btn-dark")">
                        @label
                    </a>
                }
            </nav>
        </aside>
        <main class="flex-grow-1 p-4 bg-light">
            @RenderBody()
        </main>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)

    <script>
        const s = '@TempData["SuccessMessage"]', e = '@TempData["ErrorMessage"]';
        if (s) Swal.fire({ icon: 'success', title: s, timer: 3000, showConfirmButton: false });
        if (e) Swal.fire({ icon: 'error', title: e, timer: 5000, showConfirmButton: false });
    </script>
</body>
</html>
```

## 3. Form Đăng ký (`Views/Account/Register.cshtml`)

```html
@model RoomBookingApp.Models.RegisterViewModel

@{ ViewData["Title"] = "Đăng ký"; }

<div class="row justify-content-center mt-5">
    <div class="col-md-5">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title text-center mb-4">Tạo tài khoản</h5>
                <form asp-action="Register" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2 small"></div>

                    <div class="mb-3">
                        <label asp-for="Email" class="form-label">Email</label>
                        <input asp-for="Email" type="email" class="form-control" required />
                    </div>
                    <div class="mb-3">
                        <label asp-for="Password" class="form-label">Mật khẩu</label>
                        <input asp-for="Password" type="password" class="form-control" required />
                    </div>
                    <div class="mb-3">
                        <label asp-for="ConfirmPassword" class="form-label">Nhập lại mật khẩu</label>
                        <input asp-for="ConfirmPassword" type="password" class="form-control" required />
                    </div>

                    <button type="submit" class="btn btn-primary w-100">Đăng ký</button>
                </form>
                <div class="mt-3 text-center small">
                    Đã có tài khoản? <a asp-action="Login">Đăng nhập</a>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

## 4. Form Đăng nhập (`Views/Account/Login.cshtml`)

```html
@model RoomBookingApp.Models.LoginViewModel

@{ ViewData["Title"] = "Đăng nhập"; }

<div class="row justify-content-center mt-5">
    <div class="col-md-5">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title text-center mb-4">Đăng nhập</h5>
                <form asp-action="Login" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2 small"></div>

                    <div class="mb-3">
                        <label asp-for="Email" class="form-label">Email</label>
                        <input asp-for="Email" type="email" class="form-control" required />
                    </div>
                    <div class="mb-3">
                        <label asp-for="Password" class="form-label">Mật khẩu</label>
                        <input asp-for="Password" type="password" class="form-control" required />
                    </div>
                    <div class="mb-3 form-check">
                        <input asp-for="RememberMe" class="form-check-input" />
                        <label asp-for="RememberMe" class="form-check-label">Ghi nhớ đăng nhập</label>
                    </div>

                    <button type="submit" class="btn btn-primary w-100">Đăng nhập</button>
                </form>
                <div class="mt-3 text-center small">
                    Chưa có tài khoản? <a asp-action="Register">Đăng ký</a>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

## 5. Hồ sơ người dùng (`Views/Account/Profile.cshtml`)

```html
@model RoomBookingApp.Models.UserProfileViewModel

@{ ViewData["Title"] = "Hồ sơ"; }

<div class="row justify-content-center mt-4">
    <div class="col-md-6">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title mb-4">Hồ sơ của tôi</h5>
                <form asp-action="Profile" method="post">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2 small"></div>

                    <div class="mb-3">
                        <label class="form-label">Email</label>
                        <input class="form-control" value="@Model.Email" readonly disabled />
                    </div>
                    <div class="mb-3">
                        <label asp-for="PhoneNumber" class="form-label">Số điện thoại</label>
                        <input asp-for="PhoneNumber" type="tel" class="form-control" />
                    </div>

                    <button type="submit" class="btn btn-primary">Cập nhật</button>
                </form>
            </div>
        </div>
    </div>
</div>
```

## 6. Danh sách phòng — Tra cứu (`Views/Room/Index.cshtml`)

```html
@model List<RoomBookingApp.Models.Room>

@{
    ViewData["Title"] = "Danh sách phòng họp";
    var roomLabel = new Dictionary<string, string> {
        { "RM-S-", "Small" }, { "RM-M-", "Medium" }, { "RM-L-", "Large" }, { "RM-V-", "VIP" }
    };
}

<div class="mt-4">
    <h4 class="mb-3">Danh sách phòng họp</h4>

    <form method="get" class="row g-2 mb-4">
        <div class="col-auto">
            <select name="minCapacity" class="form-select form-select-sm">
                <option value="">Sức chứa (tất cả)</option>
                <option value="2">2+ người</option>
                <option value="5">5+ người</option>
                <option value="9">9+ người</option>
            </select>
        </div>
        <div class="col-auto">
            <input name="location" class="form-control form-control-sm" placeholder="Vị trí..." value="@Context.Request.Query["location"]" />
        </div>
        <div class="col-auto">
            <button type="submit" class="btn btn-sm btn-outline-primary">Lọc</button>
            <a asp-action="Index" class="btn btn-sm btn-outline-secondary">Xóa lọc</a>
        </div>
    </form>

    <div class="row g-3">
        @foreach (var room in Model)
        {
            var prefix = room.Id?.Length >= 5 ? room.Id[..5] : "";
            <div class="col-md-6 col-lg-4">
                <div class="card shadow-sm h-100">
                    <img src="@(room.ImageUrl ?? "/images/no-image.jpg")" class="card-img-top" alt="@room.Name"
                         style="height: 180px; object-fit: cover;" />
                    <div class="card-body d-flex flex-column">
                        <div class="d-flex justify-content-between align-items-start mb-1">
                            <h5 class="card-title mb-0">@room.Name</h5>
                            @if (roomLabel.TryGetValue(prefix, out var label))
                            {
                                <span class="badge bg-secondary">@label</span>
                            }
                        </div>
                        <p class="text-muted small mb-1">@room.Location</p>
                        <p class="text-muted small mb-2">Toi da @room.Capacity nguoi</p>

                        @if (room.RoomEquipments?.Any() == true)
                        {
                            <div class="mb-2">
                                @foreach (var re in room.RoomEquipments)
                                {
                                    var ok = re.Equipment?.Status == RoomBookingApp.Models.EquipmentStatus.Available;
                                    <span class="badge bg-light text-dark me-1">@(ok ? "" : "!") @re.Equipment?.Name</span>
                                }
                            </div>
                        }

                        <h6 class="text-primary mt-auto mb-3">@room.PricePerHour.ToString("N0") đ / giờ</h6>
                        <a asp-action="Create" asp-controller="Booking" asp-route-roomId="@room.Id"
                           class="btn btn-primary">Đặt phòng</a>
                    </div>
                </div>
            </div>
        }
    </div>
</div>
```

## 7. Form Đặt phòng (`Views/Booking/Create.cshtml`)

```html
@model RoomBookingApp.Models.CreateBookingViewModel

@{
    ViewData["Title"] = "Đặt phòng họp";
}

<div class="row justify-content-center mt-4">
    <div class="col-md-6">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title mb-4">Tạo yêu cầu đặt phòng</h5>
                <form asp-action="Create" method="post" id="bookingForm">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2 small"></div>
                    <input type="hidden" asp-for="RoomId" value="@ViewBag.RoomId" />

                    <div class="mb-3">
                        <label asp-for="Title" class="form-label">Tiêu đề cuộc họp</label>
                        <input asp-for="Title" class="form-control" placeholder="VD: Họp báo cáo tuần" required />
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-md-6">
                            <label asp-for="StartTime" class="form-label">Bắt đầu</label>
                            <input asp-for="StartTime" type="datetime-local" class="form-control" id="startTime" required />
                        </div>
                        <div class="col-md-6">
                            <label asp-for="EndTime" class="form-label">Kết thúc</label>
                            <input asp-for="EndTime" type="datetime-local" class="form-control" id="endTime" required />
                        </div>
                    </div>
                    <div id="timeError" class="text-danger small mb-2 d-none"></div>

                    <div class="mb-3">
                        <label asp-for="Description" class="form-label">Ghi chú</label>
                        <textarea asp-for="Description" class="form-control" rows="3" placeholder="Yêu cầu thêm nước uống, micro..."></textarea>
                    </div>

                    <button type="submit" class="btn btn-primary w-100">Gửi yêu cầu</button>
                    <a asp-action="Index" asp-controller="Room" class="btn btn-outline-secondary w-100 mt-2">Quay lại</a>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        const startEl = document.getElementById('startTime');
        const endEl = document.getElementById('endTime');
        const errEl = document.getElementById('timeError');

        function validate() {
            const start = new Date(startEl.value);
            const end = new Date(endEl.value);
            const now = new Date();

            if (start <= now) { errEl.textContent = 'Thời gian bắt đầu phải lớn hơn thời gian hiện tại.'; errEl.classList.remove('d-none'); return false; }
            if (end <= start) { errEl.textContent = 'Thời gian kết thúc phải sau thời gian bắt đầu.'; errEl.classList.remove('d-none'); return false; }
            errEl.classList.add('d-none');
            return true;
        }

        startEl.addEventListener('change', validate);
        endEl.addEventListener('change', validate);

        document.getElementById('bookingForm').addEventListener('submit', function(e) {
            if (!validate()) e.preventDefault();
        });
    </script>
}
```

## 8. Lịch sử đặt phòng (`Views/Booking/MyBookings.cshtml`)

```html
@model List<RoomBookingApp.Models.Booking>

@{
    ViewData["Title"] = "Lịch sử đặt phòng";
    var st = new Dictionary<int, string> { {0,"Chờ duyệt"},{1,"Đã duyệt"},{2,"Từ chối"},{3,"Đã hủy"} };
    var sb = new Dictionary<int, string> { {0,"bg-warning text-dark"},{1,"bg-success"},{2,"bg-danger"},{3,"bg-secondary"} };
    var pt = new Dictionary<int, string> { {0,"Chưa TT"},{1,"Đã TT"},{2,"Hoàn tiền"} };
    var pb = new Dictionary<int, string> { {0,"bg-secondary"},{1,"bg-success"},{2,"bg-info"} };
}

<div class="mt-4">
    <h4 class="mb-3">Lịch sử đặt phòng</h4>

    @if (!Model.Any())
    {
        <div class="alert alert-info">Bạn chưa có đơn đặt phòng nào.</div>
    }
    else
    {
        <div class="table-responsive">
            <table class="table table-hover align-middle">
                <thead class="table-light">
                    <tr>
                        <th>Phòng</th>
                        <th>Tiêu đề</th>
                        <th>Bắt đầu</th>
                        <th>Kết thúc</th>
                        <th>Giá</th>
                        <th>Trạng thái</th>
                        <th>TT</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var b in Model)
                    {
                        <tr>
                            <td>@b.Room?.Name</td>
                            <td>@b.Title</td>
                            <td>@b.StartTime.ToLocalTime().ToString("dd/MM HH:mm")</td>
                            <td>@b.EndTime.ToLocalTime().ToString("HH:mm")</td>
                            <td>@b.TotalPrice.ToString("N0") đ</td>
                            <td><span class="badge @sb[(int)b.Status]">@st[(int)b.Status]</span></td>
                            <td><span class="badge @pb[(int)b.PaymentStatus]">@pt[(int)b.PaymentStatus]</span></td>
                            <td>
                                @if (b.Status == BookingStatus.Pending)
                                {
                                    <button type="button" class="btn btn-outline-danger btn-sm"
                                            onclick="confirmCancel(@b.Id, '@b.Room?.Name')">Hủy</button>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }

    <a asp-action="Index" asp-controller="Room" class="btn btn-primary mt-2">Đặt phòng mới</a>
</div>

@section Scripts {
    <script>
        function confirmCancel(id, roomName) {
            Swal.fire({
                title: 'Hủy đặt phòng?',
                text: `Bạn có chắc hủy đơn tại ${roomName}?`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Hủy đơn',
                cancelButtonText: 'Giữ lại'
            }).then(r => {
                if (r.isConfirmed) {
                    const f = document.createElement('form');
                    f.method = 'post';
                    f.action = `/Booking/Cancel/${id}`;
                    const token = document.createElement('input');
                    token.name = '__RequestVerificationToken';
                    token.value = '@Html.AntiForgeryToken()'.match(/value="([^"]+)"/)?.[1] || '';
                    f.appendChild(token);
                    document.body.appendChild(f);
                    f.submit();
                }
            });
        }
    </script>
}
```

## 9. Admin — Dashboard (`Areas/Admin/Views/Dashboard/Index.cshtml`)

```html
@model RoomBookingApp.Models.AdminDashboardViewModel

@{ ViewData["Title"] = "Dashboard"; }

<h4 class="mb-4">Tổng quan</h4>

<div class="row g-3 mb-4">
    <div class="col-md-3">
        <div class="card border-0 shadow-sm" style="border-left: 4px solid var(--primary);">
            <div class="card-body">
                <div class="text-muted small">Tổng đơn</div>
                <div class="fs-4 fw-bold">@Model.TotalBookings</div>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card border-0 shadow-sm" style="border-left: 4px solid var(--warning);">
            <div class="card-body">
                <div class="text-muted small">Chờ duyệt</div>
                <div class="fs-4 fw-bold">@Model.PendingBookings</div>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card border-0 shadow-sm" style="border-left: 4px solid var(--success);">
            <div class="card-body">
                <div class="text-muted small">Doanh thu</div>
                <div class="fs-4 fw-bold">@Model.TotalRevenue.ToString("N0") đ</div>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card border-0 shadow-sm" style="border-left: 4px solid var(--gray-500);">
            <div class="card-body">
                <div class="text-muted small">Người dùng</div>
                <div class="fs-4 fw-bold">@Model.TotalUsers</div>
            </div>
        </div>
    </div>
</div>

<div class="card shadow-sm">
    <div class="card-header bg-white">Phòng được đặt nhiều nhất</div>
    <div class="card-body">
        @if (Model.TopRooms?.Any() == true)
        {
            <table class="table table-sm mb-0">
                <thead><tr><th>Phòng</th><th>Số lần đặt</th></tr></thead>
                <tbody>
                    @foreach (var r in Model.TopRooms)
                    {
                        <tr><td>@r.RoomName</td><td>@r.Count</td></tr>
                    }
                </tbody>
            </table>
        }
        else
        {
            <p class="text-muted mb-0">Chưa có dữ liệu.</p>
        }
    </div>
</div>
```

## 10. Admin — Danh sách phòng (`Areas/Admin/Views/Room/Index.cshtml`)

```html
@model List<RoomBookingApp.Models.Room>

@{ ViewData["Title"] = "Quản lý phòng"; }

<div class="d-flex justify-content-between align-items-center mb-3">
    <h4 class="mb-0">Quản lý phòng</h4>
    <a asp-action="Create" class="btn btn-primary btn-sm">+ Thêm phòng</a>
</div>

<div class="table-responsive">
    <table class="table table-hover align-middle">
        <thead class="table-light">
            <tr>
                <th>Tên</th>
                <th>Vị trí</th>
                <th>Sức chứa</th>
                <th>Giá / giờ</th>
                <th>Trạng thái</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            @foreach (var room in Model)
            {
                <tr>
                    <td>@room.Name</td>
                    <td>@room.Location</td>
                    <td>@room.Capacity</td>
                    <td>@room.PricePerHour.ToString("N0") đ</td>
                    <td>
                        <span class="badge @(room.IsUnderMaintenance ? "bg-warning text-dark" : "bg-success")">
                            @(room.IsUnderMaintenance ? "Bảo trì" : "Hoạt động")
                        </span>
                    </td>
                    <td>
                        <a asp-action="ManageEquipment" asp-route-id="@room.Id" class="btn btn-outline-primary btn-sm">Thiết bị</a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

## 11. Admin — Thêm phòng (`Areas/Admin/Views/Room/Create.cshtml`)

```html
@model RoomBookingApp.Models.Room

@{ ViewData["Title"] = "Thêm phòng họp"; }

<div class="row justify-content-center mt-3">
    <div class="col-md-6">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title mb-4">Thêm phòng họp</h5>
                <form asp-action="Create" method="post" enctype="multipart/form-data">
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2 small"></div>

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label">Tên phòng</label>
                        <input asp-for="Name" class="form-control" required />
                    </div>

                    <div class="row g-2 mb-3">
                        <div class="col-md-6">
                            <label asp-for="Capacity" class="form-label">Sức chứa</label>
                            <input asp-for="Capacity" type="number" class="form-control" required />
                        </div>
                        <div class="col-md-6">
                            <label asp-for="PricePerHour" class="form-label">Giá / giờ</label>
                            <input asp-for="PricePerHour" type="number" step="1000" class="form-control" required />
                        </div>
                    </div>

                    <div class="mb-3">
                        <label asp-for="Location" class="form-label">Vị trí</label>
                        <input asp-for="Location" class="form-control" placeholder="Tầng, khu vực..." required />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Ảnh phòng</label>
                        <input type="file" name="imageFile" class="form-control" accept="image/*" />
                        <div class="form-text">JPEG/PNG, tối đa 5MB.</div>
                    </div>

                    <button type="submit" class="btn btn-primary">Lưu</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </form>
            </div>
        </div>
    </div>
</div>
```

## 12. Admin — Gán thiết bị cho phòng (`Areas/Admin/Views/Room/ManageEquipment.cshtml`)

```html
@model RoomBookingApp.Models.Room

@{ ViewData["Title"] = "Gán thiết bị"; }

<div class="row justify-content-center mt-3">
    <div class="col-md-6">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title mb-1">Thiết bị trong phòng</h5>
                <p class="text-muted small mb-3">@Model.Name</p>

                <form asp-action="ManageEquipment" method="post">
                    @Html.AntiForgeryToken()
                    <input type="hidden" name="roomId" value="@Model.Id" />

                    @if (ViewBag.AllEquipments is List<RoomBookingApp.Models.Equipment> all)
                    {
                        var assigned = Model.RoomEquipments?.Select(re => re.EquipmentId).ToHashSet() ?? new();
                        @foreach (var eq in all)
                        {
                            <div class="form-check mb-2">
                                <input class="form-check-input" type="checkbox" name="equipmentIds" value="@eq.Id"
                                       id="eq_@eq.Id" checked="@assigned.Contains(eq.Id)" />
                                <label class="form-check-label" for="eq_@eq.Id">@eq.Name</label>
                            </div>
                        }
                    }

                    <button type="submit" class="btn btn-primary mt-3">Lưu</button>
                    <a asp-action="Index" class="btn btn-outline-secondary mt-3">Quay lại</a>
                </form>
            </div>
        </div>
    </div>
</div>
```

## 13. Admin — Quản lý thiết bị (`Areas/Admin/Views/Equipment/Index.cshtml`)

```html
@model List<RoomBookingApp.Models.Equipment>

@{
    ViewData["Title"] = "Quản lý thiết bị";
    var badge = new Dictionary<int, string> {
        {0,"bg-success"}, {1,"bg-warning text-dark"}, {2,"bg-danger"}
    };
    var text = new Dictionary<int, string> {
        {0,"Sẵn sàng"}, {1,"Bảo trì"}, {2,"Hỏng"}
    };
}

<h4 class="mb-3">Quản lý thiết bị</h4>

<form asp-action="Create" method="post" class="row g-2 mb-3">
    @Html.AntiForgeryToken()
    <div class="col-auto">
        <input name="name" class="form-control form-control-sm" placeholder="Tên thiết bị" required />
    </div>
    <div class="col-auto">
        <input name="description" class="form-control form-control-sm" placeholder="Mô tả (không bắt buộc)" />
    </div>
    <div class="col-auto">
        <button type="submit" class="btn btn-primary btn-sm">+ Thêm</button>
    </div>
</form>

<div class="table-responsive">
    <table class="table table-hover align-middle">
        <thead class="table-light">
            <tr>
                <th>Tên</th>
                <th>Trạng thái</th>
                <th>Phòng</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            @foreach (var eq in Model)
            {
                <tr>
                    <td>@eq.Name</td>
                    <td><span class="badge @badge[(int)eq.Status]">@text[(int)eq.Status]</span></td>
                    <td>@string.Join(", ", eq.RoomEquipments?.Select(re => re.Room?.Name ?? "") ?? new())</td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <form asp-action="UpdateStatus" method="post" style="display:inline">
                                @Html.AntiForgeryToken()
                                <input type="hidden" name="id" value="@eq.Id" />
                                <select name="status" class="form-select form-select-sm d-inline w-auto" onchange="this.form.submit()">
                                    <option value="0" selected="@(eq.Status == 0)">Sẵn sàng</option>
                                    <option value="1" selected="@(eq.Status == 1)">Bảo trì</option>
                                    <option value="2" selected="@(eq.Status == 2)">Hỏng</option>
                                </select>
                                <input name="note" class="form-control form-control-sm d-inline w-auto" placeholder="Ghi chú..." value="@eq.Note" />
                            </form>
                            <a asp-action="Transfer" asp-route-id="@eq.Id" class="btn btn-outline-info btn-sm">Điều chuyển</a>
                            @if (User.IsInRole("Admin"))
                            {
                                <form asp-action="Delete" asp-route-id="@eq.Id" method="post"
                                      onsubmit="return confirmDelete('@eq.Name')" style="display:inline">
                                    @Html.AntiForgeryToken()
                                    <button type="submit" class="btn btn-outline-danger btn-sm">Xóa</button>
                                </form>
                            }
                        </div>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>

@section Scripts {
    <script>
        function confirmDelete(name) {
            Swal.fire({
                title: 'Xóa thiết bị?',
                text: name,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy'
            }).then(r => r.isConfirmed);
        }
    </script>
}
```

## 14. Admin — Điều chuyển thiết bị (`Areas/Admin/Views/Equipment/Transfer.cshtml`)

```html
@{ ViewData["Title"] = "Điều chuyển thiết bị"; }

<div class="row justify-content-center mt-3">
    <div class="col-md-5">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title mb-1">Điều chuyển</h5>
                <p class="text-muted small mb-3">@ViewBag.EquipmentName — hiện ở <strong>@ViewBag.CurrentRoom</strong></p>

                <form asp-action="Transfer" method="post">
                    @Html.AntiForgeryToken()
                    <input type="hidden" name="equipmentId" value="@ViewContext.RouteData.Values["id"]" />

                    <div class="mb-3">
                        <label class="form-label">Chuyển đến phòng</label>
                        <select name="targetRoomId" class="form-select" required>
                            <option value="">-- Chọn phòng --</option>
                            @foreach (var room in (List<RoomBookingApp.Models.Room>)ViewBag.Rooms)
                            {
                                <option value="@room.Id">@room.Name (Sức chứa: @room.Capacity)</option>
                            }
                        </select>
                    </div>

                    <button type="submit" class="btn btn-primary">Xác nhận</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </form>
            </div>
        </div>
    </div>
</div>
```

## 15. Admin — Duyệt đơn (`Areas/Admin/Views/Booking/Pending.cshtml`)

```html
@model List<RoomBookingApp.Models.Booking>

@{ ViewData["Title"] = "Duyệt đơn"; }

<h4 class="mb-3">Đơn chờ duyệt</h4>

@if (!Model.Any())
{
    <div class="alert alert-info">Không có đơn nào chờ duyệt.</div>
}
else
{
    <div class="table-responsive">
        <table class="table table-hover align-middle">
            <thead class="table-light">
                <tr>
                    <th>Người đặt</th>
                    <th>Phòng</th>
                    <th>Tiêu đề</th>
                    <th>Bắt đầu</th>
                    <th>Kết thúc</th>
                    <th>Tổng tiền</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                @foreach (var b in Model)
                {
                    <tr>
                        <td>@b.User?.Email</td>
                        <td>@b.Room?.Name</td>
                        <td>@b.Title</td>
                        <td>@b.StartTime.ToLocalTime().ToString("dd/MM HH:mm")</td>
                        <td>@b.EndTime.ToLocalTime().ToString("HH:mm")</td>
                        <td>@b.TotalPrice.ToString("N0") đ</td>
                        <td>
                            <div class="btn-group btn-group-sm">
                                <form asp-action="Approve" asp-route-id="@b.Id" method="post" style="display:inline">
                                    @Html.AntiForgeryToken()
                                    <button type="submit" class="btn btn-success btn-sm">Duyệt</button>
                                </form>
                                <form asp-action="Reject" asp-route-id="@b.Id" method="post" style="display:inline">
                                    @Html.AntiForgeryToken()
                                    <button type="submit" class="btn btn-danger btn-sm">Từ chối</button>
                                </form>
                            </div>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
```

## 16. Admin — Quản lý người dùng (`Areas/Admin/Views/User/Index.cshtml`)

```html
@model List<IdentityUser>

@{ ViewData["Title"] = "Quản lý người dùng"; }

<h4 class="mb-3">Người dùng</h4>

<div class="table-responsive">
    <table class="table table-hover align-middle">
        <thead class="table-light">
            <tr>
                <th>Email</th>
                <th>Vai trò</th>
                <th>SĐT</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            @foreach (var u in Model)
            {
                var roles = ((Dictionary<string, IList<string>>)ViewBag.UserRoles)[u.Id];
                <tr>
                    <td>@u.Email</td>
                    <td>@string.Join(", ", roles)</td>
                    <td>@u.PhoneNumber</td>
                    <td>
                        <a asp-action="ResetPassword" asp-route-id="@u.Id" class="btn btn-outline-warning btn-sm">Reset mật khẩu</a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

## 17. Admin — Reset mật khẩu (`Areas/Admin/Views/User/ResetPassword.cshtml`)

```html
@{ ViewData["Title"] = "Reset mật khẩu"; }

<div class="row justify-content-center mt-3">
    <div class="col-md-5">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title mb-1">Reset mật khẩu</h5>
                <p class="text-muted small mb-3">Người dùng: <strong>@ViewBag.UserEmail</strong></p>

                <form asp-action="ResetPassword" method="post">
                    @Html.AntiForgeryToken()
                    <input type="hidden" name="id" value="@ViewContext.RouteData.Values["id"]" />
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2 small"></div>

                    <div class="mb-3">
                        <label for="newPassword" class="form-label">Mật khẩu mới</label>
                        <input name="newPassword" type="password" class="form-control" minlength="6" required />
                        <div class="form-text">Ít nhất 6 ký tự.</div>
                    </div>

                    <button type="submit" class="btn btn-warning">Cập nhật</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </form>
            </div>
        </div>
    </div>
</div>
```

## 18. Admin — Quản lý ví (`Areas/Admin/Views/Wallet/Index.cshtml`)

```html
@model List<RoomBookingApp.Models.Wallet>

@{
    ViewData["Title"] = "Quản lý ví";
    var allUsers = (List<IdentityUser>)ViewBag.AllUsers;
}

<h4 class="mb-3">Ví tiền</h4>

<div class="table-responsive">
    <table class="table table-hover align-middle">
        <thead class="table-light">
            <tr>
                <th>Email</th>
                <th>Số dư</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            @foreach (var u in allUsers)
            {
                var wallet = Model.FirstOrDefault(w => w.UserId == u.Id);
                <tr>
                    <td>@u.Email</td>
                    <td class="@(wallet?.Balance > 0 ? "text-success fw-bold" : "text-muted")">
                        @(wallet?.Balance.ToString("N0") ?? "0") đ
                    </td>
                    <td>
                        <a asp-action="TopUp" asp-route-userId="@u.Id" class="btn btn-outline-success btn-sm">Nạp tiền</a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

## 19. Admin — Nạp tiền (`Areas/Admin/Views/Wallet/TopUp.cshtml`)

```html
@{ ViewData["Title"] = "Nạp tiền"; }

<div class="row justify-content-center mt-3">
    <div class="col-md-5">
        <div class="card shadow-sm">
            <div class="card-body p-4">
                <h5 class="card-title mb-1">Nạp tiền vào ví</h5>
                <p class="text-muted small mb-3">
                    @ViewBag.UserEmail — số dư: <strong>@ViewBag.CurrentBalance.ToString("N0") đ</strong>
                </p>

                <form asp-action="TopUp" method="post">
                    @Html.AntiForgeryToken()
                    <input type="hidden" name="userId" value="@ViewContext.RouteData.Values["userId"]" />
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger py-2 small"></div>

                    <div class="mb-3">
                        <label for="amount" class="form-label">Số tiền nạp</label>
                        <input name="amount" type="number" step="1000" class="form-control" min="1000" required />
                        <div class="form-text">Tối thiểu 1.000đ.</div>
                    </div>

                    <button type="submit" class="btn btn-success">Xác nhận</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </form>
            </div>
        </div>
    </div>
</div>
```

## Các view cần tạo

| View | Ghi chú |
|------|---------|
| `Account/Register.cshtml` | §3 |
| `Account/Login.cshtml` | §4 |
| `Account/Profile.cshtml` | §5 |
| `Room/Index.cshtml` | §6 |
| `Booking/Create.cshtml` | §7 |
| `Booking/MyBookings.cshtml` | §8 |
| `Admin/Dashboard/Index.cshtml` | §9 |
| `Admin/Room/Index.cshtml` | §10 |
| `Admin/Room/Create.cshtml` | §11 |
| `Admin/Room/ManageEquipment.cshtml` | §12 |
| `Admin/Equipment/Index.cshtml` | §13 |
| `Admin/Equipment/Transfer.cshtml` | §14 |
| `Admin/Booking/Pending.cshtml` | §15 |
| `Admin/User/Index.cshtml` | §16 |
| `Admin/User/ResetPassword.cshtml` | §17 |
| `Admin/Wallet/Index.cshtml` | §18 |
| `Admin/Wallet/TopUp.cshtml` | §19 |

## Ghi chú

- Tất cả admin view đặt trong `Areas/Admin/Views/{Controller}/`.
- Dùng `Bootstrap 5` grid, card component.
- Dùng `SweetAlert2` cho confirm dialog và TempData — đã tích hợp sẵn trong layout.
- SweetAlert2 thay thế `confirm()` native — dùng `Swal.fire({...}).then(r => r.isConfirmed)`.
- Admin layout có sidebar — copy từ §2.
- Form upload file **phải** thêm `enctype="multipart/form-data"`.
