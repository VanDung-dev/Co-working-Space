# Co-working Space Management System

Hệ thống Quản lý và Đặt phòng họp theo giờ dành cho mô hình Co-working Space, được xây dựng trên nền tảng **ASP.NET Core 10.0 MVC** và **Entity Framework Core**.

---

## Công nghệ sử dụng

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Database**: SQL Server 2022 (Docker) + Entity Framework Core 10
- **Authentication & Authorization**: ASP.NET Core Identity (RBAC: Admin, Staff, User)
- **Frontend**: Bootstrap 5, HTML5/CSS3, SweetAlert2
- **Testing**: NUnit (132 test cases)

---

## Cấu trúc dự án

```text
├── Co-working-Space/           # Web Application chính (MVC)
├── Co-working-Space.Seeder/    # CLI Tool độc lập đẩy dữ liệu mẫu vào Database
├── Co-working-Space.Nunit/     # Bộ Unit Tests kiểm thử hệ thống (132 test cases)
└── docker-compose.db.yaml      # File cấu hình SQL Server Container
```

---

## Hướng dẫn khởi chạy nhanh (Quick Start)

### 1. Yêu cầu trước khi cài đặt
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (chạy SQL Server)

---

### 2. Các bước khởi chạy

#### **Bước 1: Khởi động Database SQL Server**
```bash
docker compose -f docker-compose.db.yaml up -d
```

#### **Bước 2: Cập nhật Database (Apply Migrations)**
```bash
dotnet ef database update --project Co-working-Space/Co-working-Space.csproj
```

#### **Bước 3: Nạp dữ liệu mẫu (Seed Data)**
```bash
dotnet run --project Co-working-Space.Seeder/Co-working-Space.Seeder.csproj
```

#### **Bước 4: Khởi chạy Ứng dụng Web**
```bash
dotnet run --project Co-working-Space/Co-working-Space.csproj
```
 Truy cập ứng dụng tại: `https://localhost:7198` hoặc `http://localhost:5051`

---

## Chạy Kiểm thử (Unit Tests)

```bash
dotnet test Co-working-Space.Nunit/Co-working-Space.Nunit.csproj
```

---

## Tài khoản thử nghiệm mặc định

Sau khi chạy **Tool Seeder** ở Bước 3, hệ thống sẽ có sẵn các tài khoản thử nghiệm sau:

| Vai trò | Email | Mật khẩu | Ghi chú |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@coworking.com` | `Admin@123` | Quản lý toàn bộ hệ thống |
| **Staff** | `staff@coworking.com` | `Staff@123` | Duyệt đơn đặt phòng |
| **User 1** | `user1@coworking.com` | `User@123` | Khách đặt phòng (Số dư: **2.000.000đ**) |
| **User 2** | `user2@coworking.com` | `User@123` | Khách đặt phòng (Số dư: **1.000.000đ**) |

---

## License

Được phân phối theo **[Giấy phép MIT](LICENSE)**.