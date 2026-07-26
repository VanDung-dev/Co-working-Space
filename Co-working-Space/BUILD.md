# Build Guide

## Prerequisites

- .NET 10.0 SDK
- Docker desktop (SQL Server)

## Quick Start

```bash
# 1. Start SQL Server
docker compose -f docker-compose.db.yaml up -d

# 2. Build
dotnet build

# 3. Create migrations
dotnet ef migrations add InitialCreate --project Co-working-Space/Co-working-Space.csproj

# 4. Create database + apply migrations
dotnet ef database update --project Co-working-Space/Co-working-Space.csproj

# 5. Run
dotnet run --project Co-working-Space/Co-working-Space.csproj
```

App start lần đầu tự động seed:
- 3 roles: `Admin`, `Staff`, `User`
- Admin account: `admin@coworking.com` / `Admin@123`

## Run Tests

```bash
dotnet test Co-working-Space.Nunit/Co-working-Space.Nunit.csproj
```