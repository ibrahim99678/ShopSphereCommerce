namespace ShopSphere.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public string? Slug { get; set; }
    public int? CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Category? Category { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
