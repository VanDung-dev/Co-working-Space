# Build & Migration Guide

## Prerequisites

- .NET 10.0 SDK
- Docker desktop (SQL Server container)

## 1. Start SQL Server

```bash
docker compose -f docker-compose.db.yaml up -d
```

Container `mssql` chạy ngầm, không cần build lại nếu đã có.

## 2. Restore packages & Build

```bash
dotnet restore "Co-working Space/Co-working Space.csproj"
dotnet build "Co-working Space/Co-working Space.csproj"
```

## 3. Database Migration

Tạo migration file (chỉ lần đầu):

```bash
dotnet ef migrations add InitialCreate --project "Co-working Space/Co-working Space.csproj"
```

Cập nhật database:

```bash
dotnet ef database update --project "Co-working Space/Co-working Space.csproj"
```

> EF sẽ tự tạo database `CoWorkingSpace` nếu chưa có.

## 4. Run

```bash
dotnet run --project "Co-working Space/Co-working Space.csproj"
```

Khi app start lần đầu, seed tự động:
- 3 roles: `Admin`, `Staff`, `User`
- Tài khoản admin: `admin@coworking.com` / `Admin@123`

---

## Common Errors & Fixes

### CS0234 / CS0246 — Missing `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

```
The type or namespace name 'EntityFrameworkCore' does not exist in the namespace 'Microsoft.AspNetCore.Identity'
```

**Fix:** NuGet package này không nằm trong ASP.NET Core shared framework. Thêm vào `.csproj`:

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.10" />
```

### Login failed for user 'sa'

```
Cannot open database "CoWorkingSpace" requested by the login. The login failed.
```

**Fix:** Database chưa tồn tại. Chạy migration:

```bash
dotnet ef database update --project "Co-working Space/Co-working Space.csproj"
```

### Shadow key `UserId1` on Wallet

```
The foreign key property 'Wallet.UserId1' was created in shadow state
```

**Fix:** Dùng navigation property trong fluent API — không dùng `HasOne<IdentityUser>()` trần:

```csharp
builder.Entity<Wallet>(entity =>
{
    entity.HasKey(w => w.UserId);
    entity.HasOne(w => w.User).WithOne().HasForeignKey<Wallet>(w => w.UserId);
    entity.Navigation(w => w.User).IsRequired();
});
```

### No project found

```
No project was found. Change the current working directory or use the --project option.
```

**Fix:** Chạy từ thư mục solution root hoặc dùng `--project`:

```bash
dotnet ef database update --project "Co-working Space/Co-working Space.csproj"
```
