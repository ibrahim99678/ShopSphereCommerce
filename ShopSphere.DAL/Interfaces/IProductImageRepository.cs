using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IProductImageRepository : IGenericRepository<ProductImage>
{
    Task<IReadOnlyList<ProductImage>> GetAllByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductImage?> GetPrimaryByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<int> GetNextSortOrderAsync(int productId, CancellationToken cancellationToken = default);
}

