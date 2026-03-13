using ShopSphere.Contract.Enums;

namespace ShopSphere.Contract.Dtos;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}
