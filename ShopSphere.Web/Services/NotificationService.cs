using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Enums;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;
using DomainNotificationType = ShopSphere.Domain.Enums.NotificationType;

namespace ShopSphere.Web.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, int take = 50, CancellationToken cancellationToken = default)
    {
        take = take is < 1 or > 200 ? 50 : take;
        var entities = await _unitOfWork.Notifications.GetForUserAsync(userId, take, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task MarkReadAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Notifications.GetByIdAsync(id, cancellationToken);
        if (entity is null || entity.UserId != userId)
        {
            return;
        }

        if (entity.IsRead)
        {
            return;
        }

        entity.IsRead = true;
        _unitOfWork.Notifications.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateAsync(string userId, NotificationType type, string title, string message, int? orderId, CancellationToken cancellationToken = default)
    {
        var entity = new Notification
        {
            UserId = userId,
            Type = (DomainNotificationType)(int)type,
            Title = title,
            Message = message,
            OrderId = orderId,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto MapToDto(Notification entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Type = (NotificationType)(int)entity.Type,
            Title = entity.Title,
            Message = entity.Message,
            IsRead = entity.IsRead,
            OrderId = entity.OrderId,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
