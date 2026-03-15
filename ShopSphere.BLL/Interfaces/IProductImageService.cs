using Microsoft.AspNetCore.Http;
using ShopSphere.Contract.Dtos;

namespace ShopSphere.BLL.Interfaces;

public interface IProductImageService
{
    Task<IReadOnlyList<ProductImageDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductImageDto> UploadAsync(int productId, IFormFile file, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SetPrimaryAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> MoveAsync(int id, int direction, CancellationToken cancellationToken = default);
}

