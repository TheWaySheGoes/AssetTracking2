using AssetTracking2.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AssetTracking2.Data;

public sealed class AssetDbContext : DbContext
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Office> Offices => Set<Office>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=assets.db");

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Asset>().HasDiscriminator<string>("AssetCategory")
            .HasValue<ComputerAsset>("Computer")
            .HasValue<MobileAsset>("Mobile");
        model.Entity<Asset>().HasIndex(a => a.SerialNumber).IsUnique();
        model.Entity<Asset>().Property(a => a.PurchasePriceUsd).HasConversion<double>();
        model.Entity<Office>().Property(o => o.UsdExchangeRate).HasConversion<double>();
        model.Entity<Asset>().HasOne(a => a.Office).WithMany(o => o.Assets)
            .HasForeignKey(a => a.OfficeId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Asset>().HasOne(a => a.Employee).WithMany(e => e.AssignedAssets)
            .HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.SetNull);
    }
}
