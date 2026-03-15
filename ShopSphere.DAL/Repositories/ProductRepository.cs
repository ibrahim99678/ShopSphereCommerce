using Microsoft.EntityFrameworkCore;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly ShopSphereDbContext _dbContext;

    public ProductRepository(ShopSphereDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Product>();
        }

        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(string? search, string? brand, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sort, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 12 : pageSize;

        IQueryable<Product> query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var terms = search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(6)
                .ToArray();

            foreach (var term in terms)
            {
                var like = $"%{term}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, like)
                    || (p.Brand != null && EF.Functions.Like(p.Brand, like))
                    || (p.Sku != null && EF.Functions.Like(p.Sku, like))
                    || (p.Slug != null && EF.Functions.Like(p.Slug, like))
                    || (p.Description != null && EF.Functions.Like(p.Description, like)));
            }
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand == brand);
        }

        if (categoryId is not null)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (minPrice is not null)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice is not null)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        query = (sort ?? "newest").ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "price_desc" => query.OrderByDescending(p => p.Price).ThenByDescending(p => p.Id),
            "name_asc" => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
            "name_desc" => query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Id),
            _ => query.OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Brand != null && p.Brand != "")
            .Select(p => p.Brand!)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Sku == sku && (excludeId == null || p.Id != excludeId.Value), cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Slug == slug && (excludeId == null || p.Id != excludeId.Value), cancellationToken);
    }
}
