using ShopSphere.DAL.Interfaces;
using ShopSphere.DAL.Repositories;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ShopSphereDbContext _dbContext;

    public UnitOfWork(ShopSphereDbContext dbContext)
    {
        _dbContext = dbContext;
        Products = new GenericRepository<Product>(_dbContext);
        Orders = new OrderRepository(_dbContext);
        OrderItems = new GenericRepository<OrderItem>(_dbContext);
    }

    public IGenericRepository<Product> Products { get; }
    public IOrderRepository Orders { get; }
    public IGenericRepository<OrderItem> OrderItems { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

