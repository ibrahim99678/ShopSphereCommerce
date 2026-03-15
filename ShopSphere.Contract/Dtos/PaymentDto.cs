using ShopSphere.Contract.Enums;

namespace ShopSphere.Contract.Dtos;

public class PaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Provider { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
