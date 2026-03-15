using Microsoft.EntityFrameworkCore;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    private readonly ShopSphereDbContext _dbContext;

    public ReviewRepository(ShopSphereDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Review>> GetApprovedByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => !r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int productId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
    }
}
