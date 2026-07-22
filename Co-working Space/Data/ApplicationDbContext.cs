using Co_working_Space.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Co_working_Space.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingApproval> BookingApprovals => Set<BookingApproval>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<RoomEquipment> RoomEquipments => Set<RoomEquipment>();
    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Room>(entity =>
        {
            entity.Property(r => r.PricePerHour).HasColumnType("decimal(18,2)");
        });

        builder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(b => b.Room).WithMany(r => r.Bookings).HasForeignKey(b => b.RoomId);

            entity.HasIndex(b => new { b.RoomId, b.Status, b.StartTime, b.EndTime })
                  .HasDatabaseName("IX_Bookings_Overlap");
        });

        builder.Entity<BookingApproval>(entity =>
        {
            entity.HasOne(a => a.Booking).WithMany().HasForeignKey(a => a.BookingId);
        });

        builder.Entity<RoomEquipment>(entity =>
        {
            entity.HasKey(re => new { re.RoomId, re.EquipmentId });
            entity.HasOne(re => re.Room).WithMany(r => r.RoomEquipments).HasForeignKey(re => re.RoomId);
            entity.HasOne(re => re.Equipment).WithMany(e => e.RoomEquipments).HasForeignKey(re => re.EquipmentId);
        });

        builder.Entity<Wallet>(entity =>
        {
            entity.HasKey(w => w.UserId);
            entity.Property(w => w.Balance).HasColumnType("decimal(18,2)");
            entity.HasOne(w => w.User).WithOne().HasForeignKey<Wallet>(w => w.UserId);
            entity.Navigation(w => w.User).IsRequired();
        });
    }
}
