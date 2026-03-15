using ShopSphere.Domain.Enums;

namespace ShopSphere.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; }
    public int? OrderId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

