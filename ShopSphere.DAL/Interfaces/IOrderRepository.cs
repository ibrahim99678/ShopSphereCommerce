using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllByUserIdWithItemsAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> HasDeliveredOrderWithProductAsync(string userId, int productId, CancellationToken cancellationToken = default);
}
