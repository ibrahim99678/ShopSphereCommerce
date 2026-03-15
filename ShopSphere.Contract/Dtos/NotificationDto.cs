using ShopSphere.Contract.Enums;

namespace ShopSphere.Contract.Dtos;

public class NotificationDto
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? OrderId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

