using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.BLL;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ProductDto> CreateAsync(ProductDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl
        };

        await _unitOfWork.Products.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(ProductDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Products.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.ImageUrl = dto.ImageUrl;

        _unitOfWork.Products.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _unitOfWork.Products.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProductDto MapToDto(Product entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            ImageUrl = entity.ImageUrl
        };
    }
}

