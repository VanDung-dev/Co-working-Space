# Checklist Kiểm Thử

> File này liệt kê toàn bộ test cases cho hệ thống đặt phòng họp. Xem `guildlines.md` cho tổng quan.

---

## 1. Luồng User

| # | Test Case | Steps | Expected |
|---|-----------|-------|----------|
| TC1 | Đăng ký tài khoản mới | Vào `/Account/Register`, nhập email + password, submit | Tạo user thành công, tự động login, redirect về Home, user có role `User` |
| TC2 | Đăng nhập / Đăng xuất | Vào `/Account/Login`, nhập đúng email/password | Login thành công, Logout trả về trang chủ |
| TC3 | Phân quyền User | User đăng nhập, truy cập `/Admin/Dashboard` | Redirect login hoặc 403 Forbidden |
| TC4 | Tra cứu phòng — lọc sức chứa | Vào `/Room/Index?minCapacity=5` | Chỉ hiển thị phòng Medium (5–8), Large (9–15), VIP (2–6) |
| TC5 | Tra cứu phòng — lọc vị trí | Vào `/Room/Index?location=Tầng 2` | Chỉ hiển thị phòng có Location chứa "Tầng 2" |
| TC6 | Đặt phòng thành công | Chọn phòng, nhập khung giờ hợp lệ (tương lai), submit | Tạo booking thành công, redirect `MyBookings`, thấy đơn `Pending` |
| TC7 | Đặt phòng — trùng lịch | Đặt phòng A 09:00–10:00, sau đó đặt A 09:30–10:30 | Báo lỗi "Khung giờ này đã có người đặt" |
| TC8 | Đặt phòng — giờ trong quá khứ | Nhập `StartTime` < hiện tại | Báo lỗi, không tạo booking |
| TC9 | TotalPrice tự động tính | Phòng giá 100.000/h, đặt 2 tiếng | `TotalPrice` = 200.000 (user không sửa được) |
| TC10 | Over-posting bị chặn | Postman gửi POST thêm field `Status=1&TotalPrice=0&Id=xyz` | Server bỏ qua các field đó, chỉ bind `CreateBookingViewModel` |
| TC11 | Race condition — 2 request đồng thời | 2 tab cùng bấm Gửi cho cùng phòng + cùng khung giờ | Chỉ 1 thành công, 1 báo lỗi trùng lịch |
| TC12 | Lịch sử đặt phòng | Vào `/Booking/MyBookings` | Xem danh sách đơn đã tạo, phân biệt trạng thái bằng màu |
| TC13 | Hủy đơn — đúng trạng thái | Bấm Hủy đơn `Pending` | Status thành `Cancelled` |
| TC14 | Hủy đơn — sai trạng thái | Bấm Hủy đơn `Approved` | Báo lỗi "Chỉ hủy được đơn chờ duyệt" |

---

## 2. Luồng Admin / Staff

| # | Test Case | Steps | Expected |
|---|-----------|-------|----------|
| TC15 | Admin tạo phòng mới | Vào `/Admin/Room/Create`, nhập thông tin, submit | ID tự sinh theo loại (VD: `RM-M-016`), hiển thị trong danh sách |
| TC16 | Admin sửa phòng | Vào `/Admin/Room/Edit/{id}`, sửa tên/vị trí, submit | Thông tin phòng được cập nhật |
| TC17 | Admin bảo trì phòng | Bấm nút ToggleStatus (bật/tắt `IsActive`) | Phòng `IsActive=false` không hiển thị trên tra cứu user |
| TC18 | Admin tạo thiết bị | `/Admin/Equipment`, nhập tên "Máy chiếu Epson" | ID tự sinh: `EQ-PROJ-{số}` |
| TC19 | Admin xóa thiết bị | Bấm Xóa thiết bị | Xóa khỏi danh sách |
| TC20 | Staff xem danh sách chờ duyệt | Vào `/Admin/Booking/Pending` | Hiển thị tất cả đơn `Pending`, xếp theo giờ bắt đầu |
| TC21 | Admin duyệt đơn | Bấm Approve | `Booking.Status` → `Approved`, có bản ghi trong `BookingApprovals` |
| TC22 | Admin từ chối đơn | Bấm Reject + nhập lý do | `Booking.Status` → `Rejected`, user thấy lý do trong lịch sử |
| TC23 | User thấy trạng thái mới sau duyệt | User vào `MyBookings` | Đơn đã duyệt hiển thị `Approved` / `Rejected` |
| TC24 | Admin gán thiết bị cho phòng | Vào `/Admin/Room/ManageEquipment/{id}`, chọn thiết bị, save | Lưu vào `RoomEquipment`, lần sau vào lại thấy đã được chọn |
| TC25 | Staff xem danh sách thiết bị | Staff vào `/Admin/Equipment` | Thấy tất cả thiết bị + badge trạng thái + phòng hiện tại |
| TC26 | Staff cập nhật trạng thái | Staff chọn "Hỏng" cho máy chiếu + ghi chú | Status → `Broken`, Note lưu lại |
| TC27 | Staff điều chuyển thiết bị | Staff vào Transfer, chọn phòng B | `RoomEquipment` cập nhật, thiết bị ở phòng B |
| TC28 | Admin tạo thiết bị | Admin nhập tên "Máy chiếu Epson" | Tạo thành công, Status mặc định = Available |
| TC29 | Admin xóa thiết bị | Admin bấm Xóa thiết bị | Xóa khỏi DB |
| TC30 | Staff không được tạo thiết bị | Staff gửi POST `/Admin/Equipment/Create` | 403 Forbidden |
| TC31 | Staff không được xóa thiết bị | Staff gửi POST `/Admin/Equipment/Delete` | 403 Forbidden |
| TC32 | Dashboard thống kê | Vào `/Admin/Dashboard` | Hiển thị tổng số đơn trong tháng + 5 phòng dùng nhiều nhất |
| TC33 | Staff xem danh sách phòng | Vào `/Admin/Room` | Thấy tất cả phòng + nút toggle Bảo trì |
| TC34 | Staff bật/tắt bảo trì phòng | Staff bấm ToggleStatus | `IsActive` thay đổi, có TempData thông báo |
| TC35 | Staff không tạo/sửa/xóa phòng | Staff gửi POST Create/Edit | 403 Forbidden |
| TC36 | Staff xem danh sách user | Vào `/Admin/User` | Chỉ thấy user có role `User`, không thấy Staff/Admin |
| TC37 | Staff reset password cho User | Staff vào ResetPassword, nhập mật khẩu mới | Thành công, TempData "Đã reset mật khẩu cho..." |
| TC38 | Staff không reset password cho Staff | Staff truy cập ResetPassword của Staff khác | 403 Forbidden |
| TC39 | Admin reset password cho Staff | Admin vào ResetPassword của Staff | Thành công |
| TC40 | User không truy cập được Admin | User vào `/Admin/User` | Redirect login hoặc 403 |
| TC41 | Staff nạp tiền vào ví | Staff vào `/Admin/Wallet/TopUp`, nhập 500.000 | Wallet balance tăng 500.000 |
| TC42 | Wallet auto-create khi nạp lần đầu | User chưa có Wallet, nạp 200.000 | Wallet được tạo mới, Balance = 200.000 |
| TC43 | Duyệt đơn — số dư đủ | User có 500.000, đặt phòng 200.000, Staff Approve | Status → Approved, PaymentStatus → Paid, Balance còn 300.000 |
| TC44 | Duyệt đơn — số dư không đủ | User có 100.000, đặt phòng 200.000, Staff Approve | Báo lỗi "Số dư không đủ", Status vẫn Pending |
| TC45 | Từ chối đơn — hoàn tiền | Booking đã Paid (do edge case), Staff Reject | PaymentStatus → Refunded, Balance cộng lại |
| TC46 | Admin upload ảnh khi tạo phòng | Admin vào Create, chọn file ảnh, submit | Ảnh lưu vào `wwwroot/uploads/rooms/`, `ImageUrl` ghi đường dẫn |
| TC47 | Admin thay ảnh khi sửa phòng | Admin vào Edit, chọn file ảnh mới, submit | Ảnh cũ bị xóa (hoặc ghi đè), `ImageUrl` cập nhật |
| TC48 | Phòng không có ảnh — fallback | Tạo phòng không upload ảnh | `ImageUrl` = null, view hiển thị ảnh mặc định no-image.jpg |
| TC49 | Ảnh hiển thị đúng trên Room/Index | User xem danh sách phòng | Card phòng có ảnh với `object-fit: cover`, không vỡ layout |

---

## 3. Hệ thống

| # | Test Case | Steps | Expected |
|---|-----------|-------|----------|
| TC50 | Seed Admin mặc định | Chạy app lần đầu, chưa có user nào | Tự động tạo `admin@coworking.com` / `Admin@123` với role Admin |
| TC51 | Seed role | Chạy app, kiểm tra bảng `AspNetRoles` | Có 3 role: Admin, Staff, User |
| TC52 | FK — Xóa phòng có booking | Xóa phòng đang có booking | Bị khóa bởi FK hoặc cascade xóa booking (tùy cấu hình) |
| TC53 | Index overlap | Kiểm tra execution plan của query `HasOverlapAsync` | Index seek trên `IX_Bookings_Overlap` thay vì table scan |

---

## 4. Giao diện

| # | Test Case | Steps | Expected |
|---|-----------|-------|----------|
| TC54 | Validation giờ — client | Nhập `StartTime` < hiện tại, submit form | JS alert chặn trước khi gửi lên server |
| TC55 | Validation giờ — server | Tắt JS, gửi request với giờ quá khứ | Server trả validation error, không tạo booking |
| TC56 | SweetAlert2 confirm hủy | Bấm nút Hủy đơn | Popup xác nhận "Bạn có chắc muốn hủy?" |
| TC57 | Responsive layout | Mở trên mobile (375px width) | Card form không bị vỡ, các field xếp dọc |
| TC58 | Cột PaymentStatus hiển thị trên MyBookings | User xem danh sách đơn | Thấy badge "Đã TT" / "Chưa TT" / "Hoàn tiền" |
