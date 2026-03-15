using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(string? query, string? brand, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sort, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken cancellationToken = default);
}
