using Microsoft.EntityFrameworkCore;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Repositories;

public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
{
    private readonly ShopSphereDbContext _dbContext;

    public ProductVariantRepository(ShopSphereDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductVariant>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == productId)
            .OrderByDescending(v => v.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductVariant>> GetByIdsAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<ProductVariant>();
        }

        return await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductVariants
            .AsNoTracking()
            .AnyAsync(v => v.Sku == sku && (excludeId == null || v.Id != excludeId.Value), cancellationToken);
    }

    public async Task<bool> VariantExistsAsync(int productId, string? size, string? color, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductVariants
            .AsNoTracking()
            .AnyAsync(v =>
                v.ProductId == productId
                && v.Size == size
                && v.Color == color
                && (excludeId == null || v.Id != excludeId.Value),
                cancellationToken);
    }
}

