using Microsoft.EntityFrameworkCore;

namespace KosnicaApi.Data;

using Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Apiary> Apiaries { get; set; } = null!;
    public DbSet<Hive> Hives { get; set; } = null!;
    public DbSet<Intervention> Interventions { get; set; } = null!;
    public DbSet<Treatment> Treatments { get; set; } = null!;
    public DbSet<YieldRecord> YieldRecords { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<Shipment> Shipments { get; set; } = null!;
    public DbSet<ShipmentItem> ShipmentItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Add specific foreign key restrictions if necessary (e.g. restrict delete)
        modelBuilder.Entity<Apiary>()
            .HasOne(a => a.User)
            .WithMany(u => u.Apiaries)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Hive>()
            .HasOne(h => h.Apiary)
            .WithMany(a => a.Hives)
            .HasForeignKey(h => h.ApiaryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.Hive)
            .WithMany(h => h.Interventions)
            .HasForeignKey(i => i.HiveId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<YieldRecord>()
            .HasOne(y => y.Hive)
            .WithMany(h => h.Yields)
            .HasForeignKey(y => y.HiveId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Employer)
            .WithMany()
            .HasForeignKey(u => u.EmployerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShipmentItem>()
            .HasOne(si => si.Shipment)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
