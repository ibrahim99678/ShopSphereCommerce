using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL;

public class ShopSphereDbContext : DbContext
{
    public ShopSphereDbContext(DbContextOptions<ShopSphereDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Total).HasPrecision(18, 2);
            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.Price).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}

