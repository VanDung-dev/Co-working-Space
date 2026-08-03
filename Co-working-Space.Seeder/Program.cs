using Co_working_Space.Data;
using Co_working_Space.Seeder.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=================================================");
Console.WriteLine(" 🚀 CO-WORKING SPACE DATABASE SEEDER TOOL");
Console.WriteLine("=================================================");

// 1. Setup Configuration
var basePath = Directory.GetCurrentDirectory();
var configBuilder = new ConfigurationBuilder().SetBasePath(basePath);

if (File.Exists(Path.Combine(basePath, "appsettings.json")))
{
    configBuilder.AddJsonFile("appsettings.json", optional: false);
}
else
{
    var parentSettings = Path.Combine(basePath, "..", "Co-working-Space", "appsettings.json");
    if (File.Exists(parentSettings))
    {
        configBuilder.AddJsonFile(Path.GetFullPath(parentSettings), optional: false);
    }
    else
    {
        Console.WriteLine("❌ Không tìm thấy file appsettings.json!");
        return;
    }
}

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
    options.UseSqlServer(connectionString));

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