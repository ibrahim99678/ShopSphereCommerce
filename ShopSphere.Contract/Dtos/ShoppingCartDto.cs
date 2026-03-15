namespace ShopSphere.Contract.Dtos;

public class ShoppingCartDto
{
    public int Id { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
    public decimal Total => Subtotal;
}

