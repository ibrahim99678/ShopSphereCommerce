using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
{
    Task<ShoppingCart?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> GetByIdWithItemsAsync(int cartId, CancellationToken cancellationToken = default);
}

