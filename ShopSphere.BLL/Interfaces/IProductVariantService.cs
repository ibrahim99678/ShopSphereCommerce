using ShopSphere.Contract.Dtos;

namespace ShopSphere.BLL.Interfaces;

public interface IProductVariantService
{
    Task<ProductVariantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVariantDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductVariantDto> CreateAsync(ProductVariantDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ProductVariantDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

