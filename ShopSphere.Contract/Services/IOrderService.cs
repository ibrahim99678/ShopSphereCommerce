using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Enums;

namespace ShopSphere.Contract.Services;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderAsync(
        string customerName,
        string mobile,
        string address,
        IReadOnlyList<CartItemDto> items,
        string? userId = null,
        CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default);
}
