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
5. **Dashboard Thống kê:** Chỉ Admin — Tổng số lượt đặt phòng trong tháng, Tỷ lệ phòng được sử dụng nhiều nhất.

### C. Phía System / Backend Logic (Nghiệp vụ cốt lõi)

1. **Thuật toán chống trùng lịch (Overlap Check):** Kiểm tra khoảng thời gian `[StartTime, EndTime]` của đơn mới có bị đè lên bất kỳ đơn nào `Đã duyệt` hoặc `Chờ duyệt` trong cùng phòng hay không.
2. **Phân quyền (RBAC):** `Admin` truy cập vùng `/Admin`, `User` chỉ thao tác trên giao diện Client.
3. **Password Reset Chain:** User → Staff, Staff → Admin (reset qua `Admin/User/ResetPassword`).

---

## 📂 3. Cấu trúc tài liệu

| File | Nội dung |
|------|----------|
| [`guildlines.database.md`](guildlines.database.md) | Quy tắc ID, SQL DDL đầy đủ (Rooms, Bookings, BookingApprovals, Equipment, RoomEquipment), FK, Index |
| [`guildlines.backend.md`](guildlines.backend.md) | Entity Models, IdGenerator, DbContext, Services (BookingService, RoomService, ApprovalService), Controllers (Account, Room, Booking, Admin), Program.cs + RBAC + Seed |
| [`guildlines.frontend.md`](guildlines.frontend.md) | Razor View (Create.cshtml), danh sách view cần tạo, ghi chú Bootstrap / SweetAlert2 |
| [`guildlines.testcase.md`](guildlines.testcase.md) | 52 test cases: User flow, Admin/Staff flow, System, Giao diện |

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
| **Admin — Người dùng** | | | | |
| `/Admin/User` (xem) | ❌ | ❌ | ✅ (chỉ User) | ✅ (tất cả) |
| `/Admin/User/ResetPassword` | ❌ | ❌ | ✅ (chỉ User) | ✅ (tất cả) |
| **Admin — Dashboard** | | | | |
| `/Admin/Dashboard` | ❌ | ❌ | ❌ | ✅ |

---

Bộ code mẫu bao phủ toàn bộ 12 chức năng (**A.1–A.4, B.1–B.5, C.1–C.3**) với 3 role phân quyền rõ ràng: **User**, **Staff**, **Admin**.
