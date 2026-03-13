using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);
}

