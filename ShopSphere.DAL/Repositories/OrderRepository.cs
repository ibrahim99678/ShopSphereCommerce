using Microsoft.EntityFrameworkCore;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    private readonly ShopSphereDbContext _dbContext;

    public OrderRepository(ShopSphereDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllWithItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}

