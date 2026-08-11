# Co-working Space Management System

Hệ thống Quản lý và Đặt phòng họp theo giờ dành cho mô hình Co-working Space, được xây dựng trên nền tảng **ASP.NET Core 10.0 MVC** và **Entity Framework Core**.

---

## Công nghệ sử dụng

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Database**: MySQL 8 (Docker) + Entity Framework Core 10
- **Authentication & Authorization**: ASP.NET Core Identity (RBAC: Admin, Staff, User)
- **Frontend**: Bootstrap 5, HTML5/CSS3, SweetAlert2
- **Testing**: NUnit (132 test cases)

---

## Cấu trúc dự án

```text
├── Co-working-Space/           # Web Application chính (MVC)
├── Co-working-Space.Seeder/    # CLI Tool độc lập đẩy dữ liệu mẫu vào Database
├── Co-working-Space.Nunit/     # Bộ Unit Tests kiểm thử hệ thống (132 test cases)
└── compose.yaml                # Docker Compose: App Web + MySQL Container
```

---

## Hướng dẫn khởi chạy nhanh (Quick Start)

### 1. Yêu cầu trước khi cài đặt
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (chạy MySQL)

---

### 2. Các bước khởi chạy

#### **Bước 1: Khởi động Database MySQL**
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

Ứng dụng Web có thể chạy trong một container độc lập, kết nối đến MySQL ở bên ngoài (qua biến môi trường).

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
docker run -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal;Port=3306;Database=CoWorkingSpace;User Id=coworking;Password=coworking123" \
  co-working-space
```

Truy cập ứng dụng tại: `http://localhost:8080`

> Lưu ý: MySQL phải đang chạy (Bước 1) để ứng dụng hoạt động. Nếu DB nằm trên máy khác, thay `host.docker.internal` bằng địa chỉ IP của máy đó.

### Chia sẻ container cho người khác

Xuất image thành file tar:

```bash
docker save co-working-space:latest -o co-working-space.tar
```

Người nhận mở lại bằng:

```bash
docker load -i co-working-space.tar
docker run -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal;Port=3306;Database=CoWorkingSpace;User Id=coworking;Password=coworking123" \
  co-working-space
```

---

## Deploy lên AWS (2 EC2: web + MySQL riêng)

Chạy container web trên EC2-A và container MySQL trên EC2-B (cùng VPC, không cần RDS). Cách làm theo kiểu **"đẩy nguyên image đang có"** — export `docker save` từ máy local rồi `docker load` trên EC2, **không build, không cấu hình lại gì trên cloud**.

### Kiến trúc

```text
EC2-A (web)  --private IP-->  EC2-B (MySQL container, port 3306)
  port 8080 (public, chỉ IP của bạn)
```

### Bước 0: Export image từ máy local (chỉ 2 lệnh)

```bash
docker save co-working-space:latest -o web.tar
docker save mysql:8 -o mysql.tar
```

### Bước 1: Đẩy image lên 2 EC2

```bash
scp -i <key.pem> web.tar    ubuntu@<EC2-A-public-IP>:~/
scp -i <key.pem> mysql.tar  ubuntu@<EC2-B-public-IP>:~/
scp -i <key.pem> compose.yaml ubuntu@<EC2-B-public-IP>:~/   # tái dùng config MySQL
```

### Bước 2: EC2-B — nạp & chạy MySQL container (không sửa config)

```bash
# SSH vào EC2-B, cài Docker:
apt update && apt install -y docker.io docker-compose-v2
docker load -i mysql.tar
docker compose up -d mysql    # compose thấy image đã có sẵn → không pull, dùng đúng config trong compose.yaml
```

- Mở SG của EC2-B: inbound `TCP 3306`, source = **security group ID của EC2-A** (không mở ra internet).
- Container `mysql:8` tự tạo DB `CoWorkingSpace` + user `coworking` (đã grant cho mọi host).

### Bước 3: EC2-A — nạp & chạy web container (1 lệnh)

```bash
# SSH vào EC2-A, cài Docker:
apt update && apt install -y docker.io
docker load -i web.tar

docker run -d --restart unless-stopped -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=<EC2-B-private-IP>;Port=3306;Database=CoWorkingSpace;User Id=coworking;Password=coworking123" \
  co-working-space
```

- Mở SG của EC2-A: inbound `TCP 8080`, source = IP của bạn.
- Kiểm tra kết nối DB: `nc -zv <EC2-B-private-IP> 3306` → `succeeded` là OK.
- Chỗ duy nhất phải thay là `<EC2-B-private-IP>` (không thể tránh, vì web và db nằm 2 máy khác nhau).

### Bước 4: Migration + Seed (1 lần, từ máy local qua SSH tunnel)

DB mới sẽ trống, cần nạp schema + dữ liệu mẫu. Không lộ port 3306 ra internet — dùng SSH tunnel tới EC2-B:

```bash
# Terminal 1: mở tunnel (máy local)
ssh -i <key.pem> -L 3306:localhost:3306 ubuntu@<EC2-B-public-IP>

# Terminal 2: chạy migration + seed (máy local)
dotnet ef database update --project Co-working-Space/Co-working-Space.csproj \
  --connection "Server=localhost;Port=3306;Database=CoWorkingSpace;User Id=coworking;Password=coworking123"

ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=CoWorkingSpace;User Id=coworking;Password=coworking123" \
  dotnet run --project Co-working-Space.Seeder/Co-working-Space.Seeder.csproj
```

### Bước 5: Truy cập

Vào trình duyệt: `http://<EC2-A-public-IP>:8080`, đăng nhập bằng tài khoản mẫu (mục phía dưới).

> Ghi chú:
> - Cả 2 EC2 phải **cùng VPC** (mặc định là Default VPC) — private IP mới nối được. Khác VPC/region cần VPC peering, phức tạp hơn.
> - `web.tar` ~200MB, `mysql.tar` ~500MB — scp hơi lâu một chút, kiên nhẫn.
> - Dữ liệu MySQL lưu trong volume `mysql-db` trên ổ EC2-B — nhớ snapshot nếu muốn giữ lâu dài.
> - Khuyến nghị bảo mật: lưu connection string trong AWS Secrets Manager / SSM Parameter Store thay vì hardcode trong lệnh.

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