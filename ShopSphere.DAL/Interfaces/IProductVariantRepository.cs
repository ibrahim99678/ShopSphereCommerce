using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IProductVariantRepository : IGenericRepository<ProductVariant>
{
    Task<IReadOnlyList<ProductVariant>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVariant>> GetByIdsAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> VariantExistsAsync(int productId, string? size, string? color, int? excludeId = null, CancellationToken cancellationToken = default);
}
