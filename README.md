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
└── compose.yaml                # Docker Compose: App Web + SQL Server Container
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
docker compose up -d
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

## Chạy ứng dụng Web bằng Docker (Container độc lập)

Ứng dụng Web có thể chạy trong một container độc lập, kết nối đến SQL Server ở bên ngoài (qua biến môi trường).

### Build image

```bash
docker compose build
```

hoặc build trực tiếp từ thư mục con:

```bash
docker build -t co-working-space ./Co-working-Space
```

### Chạy container

```bash
docker run -p 8080:8080 --add-host host.docker.internal:host-gateway \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=CoWorkingSpace;User Id=sa;Password=StrongPass@1234;TrustServerCertificate=True" \
  co-working-space
```

Truy cập ứng dụng tại: `http://localhost:8080`

> Lưu ý: SQL Server phải đang chạy (Bước 1) để ứng dụng hoạt động. Nếu DB nằm trên máy khác, thay `host.docker.internal` bằng địa chỉ IP của máy đó.

### Chia sẻ container cho người khác

Xuất image thành file tar:

```bash
docker save co-working-space:latest -o co-working-space.tar
```

Người nhận mở lại bằng:

```bash
docker load -i co-working-space.tar
docker run -p 8080:8080 --add-host host.docker.internal:host-gateway \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=CoWorkingSpace;User Id=sa;Password=StrongPass@1234;TrustServerCertificate=True" \
  co-working-space
```

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