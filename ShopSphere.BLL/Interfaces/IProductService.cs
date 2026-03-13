using ShopSphere.Contract.Dtos;

namespace ShopSphere.BLL.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(ProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

