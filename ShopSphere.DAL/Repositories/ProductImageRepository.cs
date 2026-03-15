using Microsoft.EntityFrameworkCore;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Repositories;

public class ProductImageRepository : GenericRepository<ProductImage>, IProductImageRepository
{
    private readonly ShopSphereDbContext _dbContext;

    public ProductImageRepository(ShopSphereDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductImage>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductImage?> GetPrimaryByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == productId && i.IsPrimary)
            .OrderBy(i => i.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetNextSortOrderAsync(int productId, CancellationToken cancellationToken = default)
    {
        var max = await _dbContext.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken);

        return (max ?? 0) + 1;
    }
}

