using Co_working_Space.Data;
using Co_working_Space.Seeder.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=================================================");
Console.WriteLine(" 🚀 CO-WORKING SPACE DATABASE SEEDER TOOL");
Console.WriteLine("=================================================");

// 1. Setup Configuration - Search robustly for appsettings.json
string? settingsPath = FindAppSettingsPath();

if (string.IsNullOrEmpty(settingsPath))
{
    Console.WriteLine("❌ Không tìm thấy file appsettings.json trong dự án!");
    return;
}

Console.WriteLine($"📄 Loading configuration from: {settingsPath}");

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile(settingsPath, optional: false)
    .AddEnvironmentVariables();

var configuration = configBuilder.Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ ConnectionStrings:DefaultConnection bị trống trong config!");
    return;
}

Console.WriteLine($"📌 Database Connection: {connectionString}");

// 2. Setup Dependency Injection
var services = new ServiceCollection();

services.AddLogging();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString, o => o.EnableRetryOnFailure()));

services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

services.AddScoped<DatabaseSeeder>();

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

try
{
    bool cleanMode = args.Contains("--clean") || args.Contains("--reset");
    await seeder.SeedAsync(cleanMode);
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Đã xảy ra lỗi trong quá trình seed data: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"    Inner Details: {ex.InnerException.Message}");
    }
}

static string? FindAppSettingsPath()
{
    var candidates = new List<string>
    {
        Path.Combine(Directory.GetCurrentDirectory(), "Co-working-Space", "appsettings.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
        Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Co-working-Space", "appsettings.json")
    };

    foreach (var path in candidates)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            return fullPath;
        }
    }

    // Fallback: traverse up directory tree to search for Co-working-Space/appsettings.json
    var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (currentDir != null)
    {
        var subPath = Path.Combine(currentDir.FullName, "Co-working-Space", "appsettings.json");
        if (File.Exists(subPath)) return Path.GetFullPath(subPath);

        var directPath = Path.Combine(currentDir.FullName, "appsettings.json");
        if (File.Exists(directPath)) return Path.GetFullPath(directPath);

        currentDir = currentDir.Parent;
    }

    return null;
}