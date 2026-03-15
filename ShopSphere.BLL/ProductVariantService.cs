using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.BLL;

public class ProductVariantService : IProductVariantService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductVariantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductVariantDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ProductVariants.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<ProductVariantDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var variants = await _unitOfWork.ProductVariants.GetAllByProductIdAsync(productId, cancellationToken);
        return variants.Select(MapToDto).ToList();
    }

    public async Task<ProductVariantDto> CreateAsync(ProductVariantDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        if (await _unitOfWork.ProductVariants.SkuExistsAsync(dto.Sku, null, cancellationToken))
        {
            throw new InvalidOperationException("SKU already exists.");
        }

        if (await _unitOfWork.ProductVariants.VariantExistsAsync(dto.ProductId, dto.Size, dto.Color, null, cancellationToken))
        {
            throw new InvalidOperationException("Variant with same Size/Color already exists.");
        }

        var entity = new ProductVariant
        {
            ProductId = dto.ProductId,
            Sku = dto.Sku,
            Size = dto.Size,
            Color = dto.Color,
            PriceOverride = dto.PriceOverride,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive
        };

        await _unitOfWork.ProductVariants.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(ProductVariantDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ProductVariants.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (entity.ProductId != dto.ProductId)
        {
            return false;
        }

        if (await _unitOfWork.ProductVariants.SkuExistsAsync(dto.Sku, dto.Id, cancellationToken))
        {
            throw new InvalidOperationException("SKU already exists.");
        }

        if (await _unitOfWork.ProductVariants.VariantExistsAsync(dto.ProductId, dto.Size, dto.Color, dto.Id, cancellationToken))
        {
            throw new InvalidOperationException("Variant with same Size/Color already exists.");
        }

        entity.Sku = dto.Sku;
        entity.Size = dto.Size;
        entity.Color = dto.Color;
        entity.PriceOverride = dto.PriceOverride;
        entity.StockQuantity = dto.StockQuantity;
        entity.IsActive = dto.IsActive;

        _unitOfWork.ProductVariants.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ProductVariants.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _unitOfWork.ProductVariants.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ProductVariantDto MapToDto(ProductVariant entity)
    {
        return new ProductVariantDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            Sku = entity.Sku,
            Size = entity.Size,
            Color = entity.Color,
            PriceOverride = entity.PriceOverride,
            StockQuantity = entity.StockQuantity,
            IsActive = entity.IsActive
        };
    }
}

