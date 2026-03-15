namespace ShopSphere.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }
    public int ShoppingCartId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ShoppingCart? ShoppingCart { get; set; }
    public Product? Product { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}

