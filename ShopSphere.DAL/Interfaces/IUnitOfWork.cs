using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<Product> Products { get; }
    IOrderRepository Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

