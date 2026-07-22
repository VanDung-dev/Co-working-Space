# Hệ thống đặt phòng họp — Co-working Space

## 🛠️ 1. Kiến trúc hệ thống & Công nghệ (Tech Stack)

* **Backend (C#):** ASP.NET Core 10.0 MVC, Entity Framework Core (EF Core) Code First, ASP.NET Core Identity (Xác thực & Phân quyền).
* **Database (SQL):** SQL Server.
* **Frontend:** Razor Views (`.cshtml`), Bootstrap 5, HTML5/CSS3, JavaScript (Fetch API / AJAX), SweetAlert2 (Thông báo mượt mà).
* **Testing:** xUnit (Unit Tests).

---

## 📋 2. Ma trận chức năng chi tiết (Functional Breakdown)

### A. Phía User (Cán bộ / Nhân viên đặt phòng)

1. **Tài khoản:** Đăng ký, Đăng nhập, Xem/Sửa thông tin cá nhân. Khi quên mật khẩu → nhờ Staff reset.
2. **Tra cứu Phòng họp:** Xem danh sách phòng họp, lọc theo *Sức chứa*, *Vị trí*, hoặc *Trang thiết bị* (Máy chiếu, Tivi, Micro...).
3. **Tạo Yêu cầu Đặt phòng:** Chọn phòng, ngày họp, giờ bắt đầu (`StartTime`) và giờ kết thúc (`EndTime`).
4. **Quản lý Lịch sử Đặt phòng:** Xem danh sách đơn đặt của bản thân, theo dõi trạng thái (`Chờ duyệt`, `Đã duyệt`, `Từ chối`), Hủy đơn khi còn ở trạng thái `Chờ duyệt`.

### B. Phía Admin / Staff (Quản trị viên hệ thống)

1. **Quản lý Phòng họp:** Chỉ Admin — Thêm mới, Sửa thông tin, Gán thiết bị cho phòng. Staff & Admin — Xem danh sách + bật/tắt trạng thái Bảo trì (`IsActive`).
2. **Quản lý Trang thiết bị:** Admin CRUD (thêm/xóa), Staff xem + cập nhật trạng thái (Sẵn sàng / Bảo trì / Hỏng) + điều chuyển thiết bị giữa các phòng.
3. **Duyệt Đặt phòng:** Admin & Staff — Xem danh sách đơn chờ xử lý, chấp nhận (`Approve`) hoặc từ chối (`Reject`) kèm nhập lý do từ chối.
4. **Quản lý người dùng:** Staff xem danh sách User + reset mật khẩu. Admin xem tất cả + reset mật khẩu cho cả Staff.
5. **Quản lý ví tiền (Wallet):** Staff & Admin — Xem số dư, nạp tiền vào ví user. Hệ thống tự trừ khi duyệt đơn.
6. **Dashboard Thống kê:** Chỉ Admin — Tổng số lượt đặt phòng trong tháng, Tỷ lệ phòng được sử dụng nhiều nhất.

### C. Phía System / Backend Logic (Nghiệp vụ cốt lõi)

1. **Thuật toán chống trùng lịch (Overlap Check):** Kiểm tra khoảng thời gian `[StartTime, EndTime]` của đơn mới có bị đè lên bất kỳ đơn nào `Đã duyệt` hoặc `Chờ duyệt` trong cùng phòng hay không.
2. **Phân quyền (RBAC):** `Admin` truy cập vùng `/Admin`, `User` chỉ thao tác trên giao diện Client.
3. **Password Reset Chain:** User → Staff, Staff → Admin (reset qua `Admin/User/ResetPassword`).
4. **Ví tiền (Wallet):** Mỗi user có 1 ví, Staff nạp tiền tại quầy. Khi duyệt đơn, hệ thống tự trừ `TotalPrice` khỏi ví. Từ chối đơn → hoàn tiền.

---

## 📂 3. Cấu trúc tài liệu

| File | Nội dung |
|------|----------|
| [`guildlines.database.md`](guildlines.database.md) | Quy tắc ID, SQL DDL đầy đủ (Rooms, Bookings, BookingApprovals, Equipment, Wallets, RoomEquipment), FK, Index |
| [`guildlines.backend.md`](guildlines.backend.md) | Entity Models, IdGenerator, DbContext, Services (BookingService, RoomService, ApprovalService), Controllers (Account, Room, Booking, Admin), Program.cs + RBAC + Seed |
| [`guildlines.frontend.md`](guildlines.frontend.md) | Razor View (Create.cshtml), danh sách view cần tạo, ghi chú Bootstrap / SweetAlert2 |
| [`guildlines.testcase.md`](guildlines.testcase.md) | 58 test cases: User flow, Admin/Staff flow, System, Giao diện |

---

## 📋 4. Ma trận phân quyền (Route Access Matrix)

| Route | Guest | User | Staff | Admin |
|-------|-------|------|-------|-------|
| **Tài khoản** | | | | |
| `/Account/Register` | ✅ | — | — | — |
| `/Account/Login` | ✅ | — | — | — |
| `/Account/Profile` | — | ✅ | ✅ | ✅ |
| **Tra cứu phòng** | | | | |
| `/Room/Index` | ✅ | ✅ | ✅ | ✅ |
| **Đặt phòng** | | | | |
| `/Booking/Create` | — | ✅ | ✅ | ✅ |
| `/Booking/MyBookings` | — | ✅ | ✅ | ✅ |
| `/Booking/Cancel` | — | ✅ | ✅ | ✅ |
| **Admin — Phòng** | | | | |
| `/Admin/Room` (xem) | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Room/ToggleStatus` | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Room/Create` | ❌ | ❌ | ❌ | ✅ |
| `/Admin/Room/Edit` | ❌ | ❌ | ❌ | ✅ |
| `/Admin/Room/ManageEquipment` | ❌ | ❌ | ❌ | ✅ |
| **Admin — Thiết bị** | | | | |
| `/Admin/Equipment` (xem) | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Equipment/UpdateStatus` | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Equipment/Transfer` | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Equipment/Create` | ❌ | ❌ | ❌ | ✅ |
| `/Admin/Equipment/Delete` | ❌ | ❌ | ❌ | ✅ |
| **Admin — Duyệt đơn** | | | | |
| `/Admin/Booking/Pending` | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Booking/Approve` | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Booking/Reject` | ❌ | ❌ | ✅ | ✅ |
| **Admin — Ví tiền** | | | | |
| `/Admin/Wallet` (xem) | ❌ | ❌ | ✅ | ✅ |
| `/Admin/Wallet/TopUp` | ❌ | ❌ | ✅ | ✅ |
| **Admin — Người dùng** | | | | |
| `/Admin/User` (xem) | ❌ | ❌ | ✅ (chỉ User) | ✅ (tất cả) |
| `/Admin/User/ResetPassword` | ❌ | ❌ | ✅ (chỉ User) | ✅ (tất cả) |
| **Admin — Dashboard** | | | | |
| `/Admin/Dashboard` | ❌ | ❌ | ❌ | ✅ |

---

Bộ code mẫu bao phủ toàn bộ 13 chức năng (**A.1–A.4, B.1–B.6, C.1–C.4**) với 3 role phân quyền rõ ràng: **User**, **Staff**, **Admin**.

---

## ✅ 5. Checklist triển khai (từ guildlines con)

Đánh dấu `[x]` khi hoàn thành từng mục. Chi tiết code/thực hiện tại file tương ứng.

### Database (`guildlines.database.md`)

- [x] Tạo project + cấu hình SQL Server connection string
- [x] Tạo migration / chạy DDL: Rooms, Bookings, BookingApprovals, Equipment, Wallets, RoomEquipment
- [x] Thêm FK: Bookings→Rooms, Bookings→AspNetUsers, Approvals→Bookings, Approvals→AspNetUsers, Wallets→AspNetUsers
- [x] Thêm index `IX_Bookings_Overlap`
- [x] Seed Admin mặc định (`admin@coworking.com` / `Admin@123`)

### Backend Models + DbContext (`guildlines.backend.md` §1–2)

- [x] `IdGenerator` — service sinh ID tự động
- [x] Enums: `BookingStatus`, `EquipmentStatus`, `PaymentStatus`
- [x] Models: `Room`, `Booking`, `BookingApproval`, `Equipment`, `RoomEquipment`, `Wallet`
- [x] ViewModels: `CreateBookingViewModel`, `RegisterViewModel`, `LoginViewModel`, `ProfileViewModel`
- [x] `ApplicationDbContext` — 6 DbSets + cấu hình PK/HK

### Backend Services (`guildlines.backend.md` §3–5)

- [x] `BookingService` — `HasOverlapAsync` + `CreateBookingAsync` (Serializable transaction)
- [x] `RoomService` — `SearchAsync` (Include RoomEquipments + bộ lọc capacity/location/equipment)
- [x] `ApprovalService` — `GetPendingAsync`, `ApproveAsync` (trừ Wallet), `RejectAsync` (hoàn tiền nếu đã trừ)

### Backend Controllers — User (`guildlines.backend.md` §6–8)

- [ ] `AccountController` — Register, Login, Logout, Profile (Identity)
- [ ] `RoomController` — Index (tra cứu + lọc phòng)
- [ ] `BookingController` — Create, MyBookings, Cancel (chỉ hủy khi Pending)

### Backend Controllers — Admin (`guildlines.backend.md` §9–14)

- [ ] `Admin/RoomController` — CRUD phòng + ToggleStatus (Staff xem/toggle, Admin full)
- [ ] `Admin/EquipmentController` — CRUD thiết bị + UpdateStatus + Transfer
- [ ] `Admin/BookingController` — Pending + Approve/Reject
- [ ] `Admin/WalletController` — Index + TopUp
- [ ] `Admin/UserController` — Index + ResetPassword (Staff→User, Admin→all)
- [ ] `Admin/DashboardController` — thống kê số đơn + phòng dùng nhiều

### Program.cs + Seed (`guildlines.backend.md` §15)

- [x] Cấu hình DI: DbContext, Identity, Services
- [x] Cấu hình Authorization policies: `AdminOnly`, `StaffOrAdmin`
- [x] Seed 3 roles (Admin, Staff, User) + Admin default account
- [x] Routing: area + default

### Frontend Views (`guildlines.frontend.md`)

- [ ] `_Layout.cshtml` — navbar + TempData (SweetAlert2)
- [ ] `Account/Register.cshtml` — form đăng ký
- [ ] `Account/Login.cshtml` — form đăng nhập
- [ ] `Account/Profile.cshtml` — xem/sửa SĐT
- [ ] `Room/Index.cshtml` — danh sách phòng + ảnh + badge loại phòng + equipment
- [ ] `Booking/Create.cshtml` — form đặt phòng + JS validation giờ
- [ ] `Booking/MyBookings.cshtml` — lịch sử + nút hủy + cột PaymentStatus
- [ ] `Admin/Room/Index.cshtml` — quản lý phòng + nút Bảo trì
- [ ] `Admin/Room/Create.cshtml` — thêm phòng + upload ảnh (`enctype`)
- [ ] `Admin/Room/ManageEquipment.cshtml` — gán thiết bị cho phòng
- [ ] `Admin/Equipment/Index.cshtml` — danh sách + cập nhật trạng thái + nút điều chuyển
- [ ] `Admin/Equipment/Transfer.cshtml` — form chuyển phòng
- [ ] `Admin/Booking/Pending.cshtml` — danh sách chờ + nút Duyệt/Từ chối
- [ ] `Admin/Wallet/Index.cshtml` — danh sách số dư + nút Nạp tiền
- [ ] `Admin/Wallet/TopUp.cshtml` — form nạp tiền
- [ ] `Admin/User/Index.cshtml` — quản lý user + nút Reset Password
- [ ] `Admin/User/ResetPassword.cshtml` — form nhập mật khẩu mới
- [ ] `Admin/Dashboard/Index.cshtml` — thống kê

### Unit Tests (`guildlines.backend.md` §16)

- [ ] `OverlapLogicTests` — HasOverlapAsync: overlap, no overlap, exclude cancelled

### Test Cases (`guildlines.testcase.md`)

- [ ] User flow (TC01–TC14)
- [ ] Admin/Staff flow (TC15–TC49)
- [ ] System (TC50–TC53)
- [ ] Giao diện (TC54–TC58)

### Deployment

- [ ] Kiểm tra route matrix quyền truy cập đúng (Guest/User/Staff/Admin)
- [ ] Kiểm tra FK + index overlap trên SQL Server
- [ ] Kiểm tra SweetAlert2 + TempData render
- [ ] Kiểm tra responsive trên mobile (375px)
