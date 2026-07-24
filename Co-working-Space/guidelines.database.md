# Database Design & ID Conventions

> File này chứa quy tắc đặt ID, SQL DDL, ràng buộc và index. Xem `guildlines.md` cho tổng quan hệ thống.

---

## 1. Quy tắc đặt ID (Human-Readable Identifiers)

Tất cả ID có cấu trúc: `{PREFIX}-{PHÂN_LOẠI}-{SỐ_THỨ_TỰ}`. Staff nhìn prefix là biết ngay đối tượng nào.

### 1.1. Người dùng (Users / Staff / Admin)

| Role | Prefix | Ví dụ |
|------|--------|-------|
| User thường | `USR-` | `USR-0001` |
| Staff | `STF-` | `STF-0042` |
| Admin | `ADM-` | `ADM-0007` |

Lưu trong `AspNetUsers.Id` (dạng `NVARCHAR(20)`).

### 1.2. Phòng họp (Rooms)

| Loại phòng | Prefix | Sức chứa | Ví dụ |
|------------|--------|----------|-------|
| Small | `RM-S-` | 2–4 người | `RM-S-001` |
| Medium | `RM-M-` | 5–8 người | `RM-M-015` |
| Large | `RM-L-` | 9–15 người | `RM-L-008` |
| VIP | `RM-V-` | 2–6 người (nội thất cao cấp) | `RM-V-003` |

### 1.3. Thiết bị (Equipment)

| Loại | Prefix | Ví dụ |
|------|--------|-------|
| Máy chiếu (Projector) | `EQ-PROJ-` | `EQ-PROJ-002` |
| Tivi (TV) | `EQ-TV-` | `EQ-TV-005` |
| Micro (Microphone) | `EQ-MIC-` | `EQ-MIC-012` |
| Bảng trắng (Whiteboard) | `EQ-WB-` | `EQ-WB-001` |
| Loa (Speaker) | `EQ-SPK-` | `EQ-SPK-003` |
| Camera | `EQ-CAM-` | `EQ-CAM-001` |
| Đầu thu HDMI (Capture) | `EQ-CAP-` | `EQ-CAP-001` |

### 1.4. Đặt phòng (Bookings)

Prefix: `BKG-{YYYYMMDD}-{XXX}` — gồm ngày đặt để tra cứu nhanh.

| Ví dụ | Ý nghĩa |
|-------|---------|
| `BKG-20260720-001` | Booking đầu tiên ngày 20/07/2026 |
| `BKG-20260720-042` | Booking thứ 42 trong ngày |

---

## 2. SQL DDL

### 2.1. Rooms

```sql
CREATE TABLE Rooms (
    Id NVARCHAR(20) PRIMARY KEY,       -- "RM-M-015"
    Name NVARCHAR(100) NOT NULL,
    Location NVARCHAR(250) NOT NULL,
    Capacity INT NOT NULL,
    PricePerHour DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL         -- "/uploads/rooms/rm-m-015.jpg"
);
```

### 2.2. Bookings

```sql
CREATE TABLE Bookings (
    Id NVARCHAR(30) PRIMARY KEY,       -- "BKG-20260720-001"
    UserId NVARCHAR(20) NOT NULL,
    RoomId NVARCHAR(20) NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,      -- 0:Pending 1:Approved 2:Rejected 3:Cancelled
    PaymentStatus INT NOT NULL DEFAULT 0, -- 0:Unpaid 1:Paid 2:Refunded
    PaidAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Bookings_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
);
```

### 2.3. BookingApprovals

```sql
CREATE TABLE BookingApprovals (
    Id NVARCHAR(30) PRIMARY KEY,       -- "APR-0001"
    BookingId NVARCHAR(30) NOT NULL,
    ApproverId NVARCHAR(20) NOT NULL,
    Status INT NOT NULL,
    Reason NVARCHAR(MAX) NULL,
    ApprovedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Approvals_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
);
```

### 2.4. Equipment

```sql
CREATE TABLE Equipment (
    Id NVARCHAR(30) PRIMARY KEY,       -- "EQ-PROJ-002"
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status INT NOT NULL DEFAULT 0,     -- 0:Sẵn sàng 1:Bảo trì 2:Hỏng
    Note NVARCHAR(MAX) NULL            -- Ghi chú tình trạng
);
```

### 2.5. Wallets

```sql
CREATE TABLE Wallets (
    UserId NVARCHAR(20) PRIMARY KEY,   -- PK, FK → AspNetUsers.Id
    Balance DECIMAL(18, 2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_Wallets_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
```

### 2.6. RoomEquipment (bảng nối)

```sql
CREATE TABLE RoomEquipment (
    RoomId NVARCHAR(20) NOT NULL,
    EquipmentId NVARCHAR(30) NOT NULL,
    PRIMARY KEY (RoomId, EquipmentId),
    CONSTRAINT FK_RoomEquipment_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
    CONSTRAINT FK_RoomEquipment_Equipment FOREIGN KEY (EquipmentId) REFERENCES Equipment(Id)
);
```

---

## 3. Ràng buộc & Index

```sql
-- FK tới AspNetUsers
ALTER TABLE Bookings ADD CONSTRAINT FK_Bookings_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id);
ALTER TABLE BookingApprovals ADD CONSTRAINT FK_Approvals_Users FOREIGN KEY (ApproverId) REFERENCES AspNetUsers(Id);
ALTER TABLE Wallets ADD CONSTRAINT FK_Wallets_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id);

-- Index chống full-scan khi check trùng lịch
CREATE INDEX IX_Bookings_Overlap ON Bookings (RoomId, Status, StartTime, EndTime) INCLUDE (Id);
```
