using ShopSphere.Contract.Dtos;

namespace ShopSphere.BLL.Interfaces;

public interface IShoppingCartService
{
    Task<ShoppingCartDto> GetAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(string? userId, string? sessionId, int productId, int? variantId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> UpdateQuantityAsync(string? userId, string? sessionId, int itemId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string? userId, string? sessionId, int itemId, CancellationToken cancellationToken = default);
    Task ClearAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default);
}

