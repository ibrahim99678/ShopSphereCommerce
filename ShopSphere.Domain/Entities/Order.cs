using ShopSphere.Domain.Enums;

namespace ShopSphere.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string CustomerName { get; set; }
    public required string Mobile { get; set; }
    public required string Address { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

