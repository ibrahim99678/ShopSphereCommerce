using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Enums;

namespace ShopSphere.Contract.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, int take = 50, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(string userId, int id, CancellationToken cancellationToken = default);
    Task CreateAsync(string userId, NotificationType type, string title, string message, int? orderId, CancellationToken cancellationToken = default);
}

