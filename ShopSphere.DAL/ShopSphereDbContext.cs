using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DAL.Identity;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL;

public class ShopSphereDbContext : IdentityDbContext<ApplicationUser>
{
    public ShopSphereDbContext(DbContextOptions<ShopSphereDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Name).HasMaxLength(200);
            entity.Property(p => p.Brand).HasMaxLength(128);
            entity.Property(p => p.Sku).HasMaxLength(64);
            entity.Property(p => p.Slug).HasMaxLength(200);
            entity.Property(p => p.ImageUrl).HasMaxLength(2048);
            entity.Property(p => p.StockQuantity).HasDefaultValue(0);
            entity.HasIndex(p => p.Sku).IsUnique().HasFilter("[Sku] IS NOT NULL");
            entity.HasIndex(p => p.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.Brand);
            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Total).HasPrecision(18, 2);
            entity.Property(o => o.UserId).HasMaxLength(450);
            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.Property(a => a.UserId).HasMaxLength(450);
            entity.Property(a => a.FullName).HasMaxLength(256);
            entity.Property(a => a.PhoneNumber).HasMaxLength(64);
            entity.Property(a => a.AddressLine1).HasMaxLength(512);
            entity.Property(a => a.AddressLine2).HasMaxLength(512);
            entity.Property(a => a.City).HasMaxLength(128);
            entity.Property(a => a.Country).HasMaxLength(128);
            entity.Property(a => a.PostalCode).HasMaxLength(32);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200);
            entity.Property(c => c.Slug).HasMaxLength(200);
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.Property(v => v.Sku).HasMaxLength(64);
            entity.Property(v => v.Size).HasMaxLength(32);
            entity.Property(v => v.Color).HasMaxLength(32);
            entity.Property(v => v.PriceOverride).HasPrecision(18, 2);
            entity.HasIndex(v => v.Sku).IsUnique();
            entity.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.Property(i => i.ImageUrl).HasMaxLength(2048);
            entity.HasIndex(i => new { i.ProductId, i.SortOrder }).IsUnique();
            entity.HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.Property(c => c.UserId).HasMaxLength(450);
            entity.Property(c => c.SessionId).HasMaxLength(64);
            entity.HasIndex(c => c.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
            entity.HasIndex(c => c.SessionId).IsUnique().HasFilter("[SessionId] IS NOT NULL");
            entity.HasMany(c => c.Items)
                .WithOne(i => i.ShoppingCart)
                .HasForeignKey(i => i.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(i => new { i.ShoppingCartId, i.ProductId, i.ProductVariantId });
            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.ProductVariant)
                .WithMany()
                .HasForeignKey(i => i.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.Provider).HasMaxLength(64);
            entity.Property(p => p.TransactionId).HasMaxLength(128);
            entity.Property(p => p.GatewayReference).HasMaxLength(256);
            entity.Property(p => p.SessionId).HasMaxLength(64);
            entity.Property(p => p.RawResponse).HasMaxLength(4000);
            entity.HasIndex(p => p.OrderId).IsUnique();
            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.UserId).HasMaxLength(450);
            entity.Property(r => r.ReviewerName).HasMaxLength(256);
            entity.Property(r => r.Title).HasMaxLength(128);
            entity.Property(r => r.Comment).HasMaxLength(2000);
            entity.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
            entity.HasIndex(r => new { r.ProductId, r.IsApproved });
            entity.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.UserId).HasMaxLength(450);
            entity.Property(n => n.Title).HasMaxLength(256);
            entity.Property(n => n.Message).HasMaxLength(4000);
            entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAtUtc });
            entity.HasIndex(n => new { n.Type, n.CreatedAtUtc });
        });
    }
}
