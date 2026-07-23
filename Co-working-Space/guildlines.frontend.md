# Frontend Implementation (Razor View & JS)

> File này chứa code mẫu giao diện người dùng. Xem `guildlines.md` cho tổng quan hệ thống.

---

## 1. Form Đặt phòng (`Views/Booking/Create.cshtml`)

```html
@model RoomBookingApp.Models.CreateBookingViewModel

@{
    ViewData["Title"] = "Đặt phòng họp";
}

<div class="container mt-4" style="max-width: 600px;">
    <div class="card shadow-sm border-0 rounded-3">
        <div class="card-header bg-primary text-white py-3">
            <h5 class="card-title mb-0">Tạo Yêu Cầu Đặt Phòng</h5>
        </div>
        <div class="card-body p-4">
            <form asp-action="Create" method="post" id="bookingForm">
                <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>
                <input type="hidden" asp-for="RoomId" value="@ViewBag.RoomId" />

                <div class="mb-3">
                    <label asp-for="Title" class="form-label fw-bold">Tên / Tiêu đề cuộc họp</label>
                    <input asp-for="Title" class="form-control" placeholder="Ví dụ: Họp báo cáo tiến độ tuần" required />
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-md-6">
                        <label asp-for="StartTime" class="form-label fw-bold">Thời gian bắt đầu</label>
                        <input asp-for="StartTime" type="datetime-local" class="form-control" id="startTime" required />
                    </div>
                    <div class="col-md-6">
                        <label asp-for="EndTime" class="form-label fw-bold">Thời gian kết thúc</label>
                        <input asp-for="EndTime" type="datetime-local" class="form-control" id="endTime" required />
                    </div>
                </div>

                <div class="mb-3">
                    <label asp-for="Description" class="form-label fw-bold">Ghi chú / Mục đích</label>
                    <textarea asp-for="Description" class="form-control" rows="3" placeholder="Yêu cầu thêm nước uống, micro..."></textarea>
                </div>

                <div class="d-grid gap-2">
                    <button type="submit" class="btn btn-primary btn-lg">Gửi Yêu Cầu</button>
                    <a asp-action="Index" asp-controller="Room" class="btn btn-outline-secondary">Quay lại</a>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        document.getElementById('bookingForm').addEventListener('submit', function(e) {
            const start = new Date(document.getElementById('startTime').value);
            const end = new Date(document.getElementById('endTime').value);
            const now = new Date();

            if (start <= now) {
                e.preventDefault();
                alert('Thời gian bắt đầu phải lớn hơn thời gian hiện tại!');
            } else if (end <= start) {
                e.preventDefault();
                alert('Thời gian kết thúc phải sau thời gian bắt đầu!');
            }
        });
    </script>
}
```

---

## 2. Các View khác cần tạo

| View | Controller | Gợi ý nội dung |
|------|------------|---------------|
| `Account/Register.cshtml` | AccountController | Form đăng ký (Email, Password, ConfirmPassword) |
| `Account/Login.cshtml` | AccountController | Form đăng nhập (Email, Password, RememberMe) |
| `Account/Profile.cshtml` | AccountController | Hiển thị Email + sửa PhoneNumber |
| `Room/Index.cshtml` | RoomController | Danh sách phòng + bộ lọc (Capacity, Location) |
| `Booking/MyBookings.cshtml` | BookingController | Bảng danh sách đơn + nút Hủy |
| `Admin/Room/Index.cshtml` | Admin RoomController | Bảng CRUD phòng + nút Bảo trì |
| `Admin/Room/Create.cshtml` | Admin RoomController | Form thêm phòng + upload ảnh (`enctype="multipart/form-data"`) |
| `Admin/Room/ManageEquipment.cshtml` | Admin RoomController | Checkbox danh sách thiết bị để gán cho phòng |
| `Admin/Equipment/Index.cshtml` | Admin EquipmentController | Bảng danh sách thiết bị + badge trạng thái + nút cập nhật/điều chuyển |
| `Admin/Equipment/Transfer.cshtml` | Admin EquipmentController | Form chọn phòng đích để chuyển thiết bị |
| `Admin/Booking/Pending.cshtml` | Admin BookingController | Danh sách chờ + nút Duyệt/Từ chối |
| `Admin/Dashboard/Index.cshtml` | Admin DashboardController | Thống kê số đơn, phòng dùng nhiều |
| `Admin/User/Index.cshtml` | Admin UserController | Bảng danh sách user + nút Reset Password |
| `Admin/User/ResetPassword.cshtml` | Admin UserController | Form nhập mật khẩu mới |
| `Admin/Wallet/Index.cshtml` | Admin WalletController | Bảng danh sách user + số dư + nút Nạp tiền |
| `Admin/Wallet/TopUp.cshtml` | Admin WalletController | Form nhập số tiền cần nạp |

---

## 3. _Layout.cshtml — Render TempData + SweetAlert2 (`Views/Shared/_Layout.cshtml`)

```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] — Co-working Space</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body>
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
        <div class="container">
            <a class="navbar-brand" href="/">🏢 Co-working Space</a>
            <div class="navbar-nav ms-auto">
                @if (User.Identity!.IsAuthenticated)
                {
                    <a class="nav-link" asp-controller="Booking" asp-action="MyBookings">Đơn của tôi</a>
                    <a class="nav-link" asp-controller="Account" asp-action="Profile">Hồ sơ</a>
                    <form asp-controller="Account" asp-action="Logout" method="post" class="d-inline">
                        @Html.AntiForgeryToken()
                        <button type="submit" class="btn nav-link">Đăng xuất</button>
                    </form>
                }
                else
                {
                    <a class="nav-link" asp-controller="Account" asp-action="Register">Đăng ký</a>
                    <a class="nav-link" asp-controller="Account" asp-action="Login">Đăng nhập</a>
                }
            </div>
        </div>
    </nav>

    <main class="container py-3">
        @if (TempData["SuccessMessage"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show">@TempData["SuccessMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show">@TempData["ErrorMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @RenderBody()
    </main>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

> **Ghi chú:** Admin area nên dùng layout riêng (`Areas/Admin/Views/Shared/_Layout.cshtml`) với sidebar menu.

---

## 4. Form Đăng ký (`Views/Account/Register.cshtml`)

```html
@model RoomBookingApp.Models.RegisterViewModel

@{
    ViewData["Title"] = "Đăng ký tài khoản";
}

<div class="container mt-5" style="max-width: 450px;">
    <div class="card shadow-sm border-0 rounded-3">
        <div class="card-header bg-success text-white py-3">
            <h5 class="card-title mb-0">Đăng ký tài khoản</h5>
        </div>
        <div class="card-body p-4">
            <form asp-action="Register" method="post">
                <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>

                <div class="mb-3">
                    <label asp-for="Email" class="form-label fw-bold">Email</label>
                    <input asp-for="Email" type="email" class="form-control" required />
                </div>
                <div class="mb-3">
                    <label asp-for="Password" class="form-label fw-bold">Mật khẩu</label>
                    <input asp-for="Password" type="password" class="form-control" required />
                </div>
                <div class="mb-3">
                    <label asp-for="ConfirmPassword" class="form-label fw-bold">Nhập lại mật khẩu</label>
                    <input asp-for="ConfirmPassword" type="password" class="form-control" required />
                </div>

                <div class="d-grid">
                    <button type="submit" class="btn btn-success btn-lg">Đăng ký</button>
                </div>
            </form>
            <div class="mt-3 text-center">
                Đã có tài khoản? <a asp-action="Login">Đăng nhập</a>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

---

## 5. Form Đăng nhập (`Views/Account/Login.cshtml`)

```html
@model RoomBookingApp.Models.LoginViewModel

@{
    ViewData["Title"] = "Đăng nhập";
}

<div class="container mt-5" style="max-width: 450px;">
    <div class="card shadow-sm border-0 rounded-3">
        <div class="card-header bg-primary text-white py-3">
            <h5 class="card-title mb-0">Đăng nhập</h5>
        </div>
        <div class="card-body p-4">
            <form asp-action="Login" method="post">
                <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>

                <div class="mb-3">
                    <label asp-for="Email" class="form-label fw-bold">Email</label>
                    <input asp-for="Email" type="email" class="form-control" required />
                </div>
                <div class="mb-3">
                    <label asp-for="Password" class="form-label fw-bold">Mật khẩu</label>
                    <input asp-for="Password" type="password" class="form-control" required />
                </div>
                <div class="mb-3 form-check">
                    <input asp-for="RememberMe" class="form-check-input" />
                    <label asp-for="RememberMe" class="form-check-label">Ghi nhớ đăng nhập</label>
                </div>

                <div class="d-grid">
                    <button type="submit" class="btn btn-primary btn-lg">Đăng nhập</button>
                </div>
            </form>
            <div class="mt-3 text-center">
                Chưa có tài khoản? <a asp-action="Register">Đăng ký</a>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

---

## 6. Danh sách đơn đặt phòng (`Views/Booking/MyBookings.cshtml`)

```html
@model List<RoomBookingApp.Models.Booking>

@{
    ViewData["Title"] = "Lịch sử đặt phòng";
    var statusBadge = new Dictionary<int, string>
    {
        { 0, "badge bg-warning text-dark" },  // Pending
        { 1, "badge bg-success" },            // Approved
        { 2, "badge bg-danger" },             // Rejected
        { 3, "badge bg-secondary" }           // Cancelled
    };
    var statusText = new Dictionary<int, string>
    {
        { 0, "Chờ duyệt" }, { 1, "Đã duyệt" }, { 2, "Từ chối" }, { 3, "Đã hủy" }
    };
    var paymentBadge = new Dictionary<int, string>
    {
        { 0, "badge bg-secondary" },   // Unpaid
        { 1, "badge bg-success" },     // Paid
        { 2, "badge bg-info" }         // Refunded
    };
    var paymentText = new Dictionary<int, string>
    {
        { 0, "Chưa TT" }, { 1, "Đã TT" }, { 2, "Hoàn tiền" }
    };
}

<div class="container mt-4">
    <h4 class="mb-3">📋 Lịch sử đặt phòng</h4>

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
                        <th>Mã đơn</th>
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
                            <td><code>@b.Id</code></td>
                            <td>@b.Room?.Name</td>
                            <td>@b.Title</td>
                            <td>@b.StartTime.ToLocalTime().ToString("dd/MM HH:mm")</td>
                            <td>@b.EndTime.ToLocalTime().ToString("HH:mm")</td>
                            <td>@b.TotalPrice.ToString("N0") đ</td>
                            <td><span class="@statusBadge[(int)b.Status]">@statusText[(int)b.Status]</span></td>
                            <td><span class="@paymentBadge[(int)b.PaymentStatus]">@paymentText[(int)b.PaymentStatus]</span></td>
                            <td>
                                @if (b.Status == BookingStatus.Pending)
                                {
                                    <form asp-action="Cancel" asp-route-id="@b.Id" method="post"
                                          onsubmit="return confirm('Hủy đơn @b.Id?');">
                                        @Html.AntiForgeryToken()
                                        <button type="submit" class="btn btn-outline-danger btn-sm">Hủy</button>
                                    </form>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }

    <a asp-action="Index" asp-controller="Room" class="btn btn-primary mt-2">← Đặt phòng mới</a>
</div>
```

---

## 7. Quản lý thiết bị (`Areas/Admin/Views/Equipment/Index.cshtml`)

```html
@model List<RoomBookingApp.Models.Equipment>

@{
    ViewData["Title"] = "Quản lý thiết bị";
    var badgeColor = new Dictionary<int, string>
    {
        { 0, "badge bg-success" },       // Available
        { 1, "badge bg-warning text-dark" }, // Maintenance
        { 2, "badge bg-danger" }         // Broken
    };
    var statusText = new Dictionary<int, string>
    {
        { 0, "Sẵn sàng" }, { 1, "Bảo trì" }, { 2, "Hỏng" }
    };
}

<div class="container mt-4">
    <h4 class="mb-3">🔧 Quản lý thiết bị</h4>

    @if (User.IsInRole("Admin"))
    {
        <form asp-action="Create" method="post" class="row g-2 mb-3">
            @Html.AntiForgeryToken()
            <div class="col-auto">
                <input name="name" class="form-control" placeholder="Tên thiết bị" required />
            </div>
            <div class="col-auto">
                <input name="description" class="form-control" placeholder="Mô tả (không bắt buộc)" />
            </div>
            <div class="col-auto">
                <button type="submit" class="btn btn-primary">+ Thêm</button>
            </div>
        </form>
    }

    <div class="table-responsive">
        <table class="table table-hover align-middle">
            <thead class="table-light">
                <tr>
                    <th>Mã</th>
                    <th>Tên</th>
                    <th>Trạng thái</th>
                    <th>Ghi chú</th>
                    <th>Phòng hiện tại</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                @foreach (var eq in Model)
                {
                    <tr>
                        <td><code>@eq.Id</code></td>
                        <td>@eq.Name</td>
                        <td><span class="@badgeColor[(int)eq.Status]">@statusText[(int)eq.Status]</span></td>
                        <td>@eq.Note</td>
                        <td>
                            @string.Join(", ", eq.RoomEquipments.Select(re => re.Room?.Name ?? ""))
                        </td>
                        <td>
                            <div class="btn-group btn-group-sm">
                                <form asp-action="UpdateStatus" method="post" style="display:inline">
                                    @Html.AntiForgeryToken()
                                    <input type="hidden" name="id" value="@eq.Id" />
                                    <select name="status" class="form-select form-select-sm d-inline w-auto"
                                            onchange="this.form.submit()">
                                        <option value="0" selected="@(eq.Status == 0)">Sẵn sàng</option>
                                        <option value="1" selected="@(eq.Status == 1)">Bảo trì</option>
                                        <option value="2" selected="@(eq.Status == 2)">Hỏng</option>
                                    </select>
                                    <input name="note" class="form-control form-control-sm d-inline w-auto"
                                           placeholder="Ghi chú..." value="@eq.Note" />
                                </form>
                                <a asp-action="Transfer" asp-route-id="@eq.Id"
                                   class="btn btn-outline-info btn-sm">Điều chuyển</a>
                                @if (User.IsInRole("Admin"))
                                {
                                    <form asp-action="Delete" asp-route-id="@eq.Id" method="post"
                                          onsubmit="return confirm('Xóa @eq.Name?')" style="display:inline">
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
</div>
```

---

## 8. Điều chuyển thiết bị (`Areas/Admin/Views/Equipment/Transfer.cshtml`)

```html
@{
    ViewData["Title"] = "Điều chuyển thiết bị";
}

<div class="container mt-4" style="max-width: 500px;">
    <div class="card shadow-sm border-0 rounded-3">
        <div class="card-header bg-info text-white py-3">
            <h5 class="card-title mb-0">🔄 Điều chuyển: @ViewBag.EquipmentName</h5>
        </div>
        <div class="card-body p-4">
            <p class="text-muted">Hiện đang ở: <strong>@ViewBag.CurrentRoom</strong></p>

            <form asp-action="Transfer" method="post">
                @Html.AntiForgeryToken()
                <input type="hidden" name="equipmentId" value="@ViewContext.RouteData.Values["id"]" />

                <div class="mb-3">
                    <label class="form-label fw-bold">Chuyển đến phòng</label>
                    <select name="targetRoomId" class="form-select" required>
                        <option value="">-- Chọn phòng --</option>
                        @foreach (var room in (List<RoomBookingApp.Models.Room>)ViewBag.Rooms)
                        {
                            <option value="@room.Id">@room.Name (Sức chứa: @room.Capacity)</option>
                        }
                    </select>
                </div>

                <div class="d-grid gap-2">
                    <button type="submit" class="btn btn-info text-white">Xác nhận điều chuyển</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </div>
            </form>
        </div>
    </div>
</div>
```

---

## 9. Danh sách phòng — Tra cứu (`Views/Room/Index.cshtml`)

```html
@model List<RoomBookingApp.Models.Room>

@{
    ViewData["Title"] = "Danh sách phòng họp";
    var roomBadge = new Dictionary<string, string>
    {
        { "RM-S-", "badge bg-secondary" },
        { "RM-M-", "badge bg-primary" },
        { "RM-L-", "badge bg-info" },
        { "RM-V-", "badge bg-warning text-dark" }
    };
    var roomLabel = new Dictionary<string, string>
    {
        { "RM-S-", "Small" },
        { "RM-M-", "Medium" },
        { "RM-L-", "Large" },
        { "RM-V-", "VIP" }
    };
}

<div class="container mt-4">
    <h4 class="mb-3">🏢 Danh sách phòng họp</h4>

    <form method="get" class="row g-2 mb-4">
        <div class="col-auto">
            <select name="minCapacity" class="form-select">
                <option value="">Sức chứa (tất cả)</option>
                <option value="2">2+ người</option>
                <option value="5">5+ người</option>
                <option value="9">9+ người</option>
            </select>
        </div>
        <div class="col-auto">
            <input name="location" class="form-control" placeholder="Vị trí..." value="@Context.Request.Query["location"]" />
        </div>
        <div class="col-auto">
            <button type="submit" class="btn btn-outline-primary">🔍 Lọc</button>
            <a asp-action="Index" class="btn btn-outline-secondary">Xóa lọc</a>
        </div>
    </form>

    <div class="row g-4">
        @foreach (var room in Model)
        {
            var prefix = room.Id[..5];
            <div class="col-md-6 col-lg-4">
                <div class="card shadow-sm h-100 border-0 rounded-3">
                    <img src="@(room.ImageUrl ?? "/images/no-image.jpg")" class="card-img-top" alt="@room.Name"
                         style="height: 180px; object-fit: cover;" />
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <h5 class="card-title mb-0">@room.Name</h5>
                            <span class="@roomBadge.GetValueOrDefault(prefix, "badge bg-secondary")">
                                @roomLabel.GetValueOrDefault(prefix, "")
                            </span>
                        </div>
                        <p class="text-muted small mb-1">📍 @room.Location</p>
                        <p class="text-muted small mb-2">👥 Tối đa @room.Capacity người</p>

                        @if (room.RoomEquipments.Any())
                        {
                            <div class="mb-2">
                                @foreach (var re in room.RoomEquipments)
                                {
                                    var statusIcon = re.Equipment.Status == RoomBookingApp.Models.EquipmentStatus.Available ? "✅" : "⚠️";
                                    <span class="badge bg-light text-dark me-1">@statusIcon @re.Equipment.Name</span>
                                }
                            </div>
                        }

                        <h6 class="text-primary mb-3">@room.PricePerHour.ToString("N0") đ / giờ</h6>
                        <a asp-action="Create" asp-controller="Booking" asp-route-roomId="@room.Id"
                           class="btn btn-primary w-100">Đặt phòng</a>
                    </div>
                </div>
            </div>
        }
    </div>
</div>
```

---

## 10. Admin — Thêm phòng (`Areas/Admin/Views/Room/Create.cshtml`)

```html
@model RoomBookingApp.Models.Room

@{
    ViewData["Title"] = "Thêm phòng họp";
}

<div class="container mt-4" style="max-width: 600px;">
    <div class="card shadow-sm border-0 rounded-3">
        <div class="card-header bg-primary text-white py-3">
            <h5 class="card-title mb-0">➕ Thêm phòng họp</h5>
        </div>
        <div class="card-body p-4">
            <form asp-action="Create" method="post" enctype="multipart/form-data">
                <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>

                <div class="mb-3">
                    <label asp-for="Name" class="form-label fw-bold">Tên phòng</label>
                    <input asp-for="Name" class="form-control" required />
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-md-6">
                        <label asp-for="Capacity" class="form-label fw-bold">Sức chứa</label>
                        <input asp-for="Capacity" type="number" class="form-control" required />
                    </div>
                    <div class="col-md-6">
                        <label asp-for="PricePerHour" class="form-label fw-bold">Giá / giờ</label>
                        <input asp-for="PricePerHour" type="number" step="1000" class="form-control" required />
                    </div>
                </div>

                <div class="mb-3">
                    <label asp-for="Location" class="form-label fw-bold">Vị trí</label>
                    <input asp-for="Location" class="form-control" placeholder="Tầng, khu vực..." required />
                </div>

                <div class="mb-3">
                    <label class="form-label fw-bold">Ảnh phòng</label>
                    <input type="file" name="imageFile" class="form-control" accept="image/*" />
                    <div class="form-text">Chọn 1 ảnh (JPEG/PNG, tối đa 5MB).</div>
                </div>

                <div class="d-grid gap-2">
                    <button type="submit" class="btn btn-primary">Lưu</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </div>
            </form>
        </div>
    </div>
</div>
```

---

---

## 11. Admin — Quản lý người dùng (`Areas/Admin/Views/User/Index.cshtml`)

```html
@model List<IdentityUser>

@{
    ViewData["Title"] = "Quản lý người dùng";
}

<div class="container mt-4">
    <h4 class="mb-3">👥 Quản lý người dùng</h4>
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
                            <a asp-action="ResetPassword" asp-route-id="@u.Id"
                               class="btn btn-outline-warning btn-sm">🔑 Reset mật khẩu</a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
</div>
```

---

## 12. Admin — Reset mật khẩu (`Areas/Admin/Views/User/ResetPassword.cshtml`)

```html
@{
    ViewData["Title"] = "Reset mật khẩu";
}

<div class="container mt-4" style="max-width: 450px;">
    <div class="card shadow-sm border-0 rounded-3">
        <div class="card-header bg-warning text-dark py-3">
            <h5 class="card-title mb-0">🔑 Reset mật khẩu</h5>
        </div>
        <div class="card-body p-4">
            <p class="text-muted">Người dùng: <strong>@ViewBag.UserEmail</strong></p>
            <form asp-action="ResetPassword" method="post">
                @Html.AntiForgeryToken()
                <input type="hidden" name="id" value="@ViewContext.RouteData.Values["id"]" />
                <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>

                <div class="mb-3">
                    <label for="newPassword" class="form-label fw-bold">Mật khẩu mới</label>
                    <input name="newPassword" type="password" class="form-control" minlength="6" required />
                    <div class="form-text">Ít nhất 6 ký tự.</div>
                </div>

                <div class="d-grid gap-2">
                    <button type="submit" class="btn btn-warning">Cập nhật mật khẩu</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </div>
            </form>
        </div>
    </div>
</div>
```

---

---

## 13. Admin — Quản lý ví (`Areas/Admin/Views/Wallet/Index.cshtml`)

```html
@model List<RoomBookingApp.Models.Wallet>

@{
    ViewData["Title"] = "Quản lý ví";
    var allUsers = (List<IdentityUser>)ViewBag.AllUsers;
}

<div class="container mt-4">
    <h4 class="mb-3">💰 Quản lý ví tiền</h4>
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
                            <a asp-action="TopUp" asp-route-userId="@u.Id"
                               class="btn btn-outline-success btn-sm">➕ Nạp tiền</a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
</div>
```

---

## 14. Admin — Nạp tiền (`Areas/Admin/Views/Wallet/TopUp.cshtml`)

```html
@{
    ViewData["Title"] = "Nạp tiền";
}

<div class="container mt-4" style="max-width: 450px;">
    <div class="card shadow-sm border-0 rounded-3">
        <div class="card-header bg-success text-white py-3">
            <h5 class="card-title mb-0">💰 Nạp tiền vào ví</h5>
        </div>
        <div class="card-body p-4">
            <p class="text-muted">Người dùng: <strong>@ViewBag.UserEmail</strong></p>
            <p class="text-muted">Số dư hiện tại: <strong>@ViewBag.CurrentBalance.ToString("N0") đ</strong></p>

            <form asp-action="TopUp" method="post">
                @Html.AntiForgeryToken()
                <input type="hidden" name="userId" value="@ViewContext.RouteData.Values["userId"]" />
                <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>

                <div class="mb-3">
                    <label for="amount" class="form-label fw-bold">Số tiền nạp</label>
                    <input name="amount" type="number" step="1000" class="form-control" min="1000" required />
                    <div class="form-text">Tối thiểu 1.000đ.</div>
                </div>

                <div class="d-grid gap-2">
                    <button type="submit" class="btn btn-success">Xác nhận nạp</button>
                    <a asp-action="Index" class="btn btn-outline-secondary">Quay lại</a>
                </div>
            </form>
        </div>
    </div>
</div>
```

---

## 15. Ghi chú khi tạo view

- Tất cả Admin view đặt trong `Areas/Admin/Views/{Controller}/`
- Dùng `Bootstrap 5` grid (`container`, `row`, `col-*`) + card component
- Dùng `SweetAlert2` cho confirm dialog (hủy đơn, xóa thiết bị)
- Dùng `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]` — đã có sẵn render trong `_Layout.cshtml` §3
- Admin layout nên thêm sidebar: quản lý phòng, thiết bị, duyệt đơn, dashboard, quản lý user, quản lý ví
- Form có upload file (Admin Create/Edit phòng) **phải** thêm `enctype="multipart/form-data"` trong thẻ `<form>`
