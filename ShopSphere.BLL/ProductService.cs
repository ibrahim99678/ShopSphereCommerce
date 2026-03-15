using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
        var entity = await _unitOfWork.Products.GetByIdWithCategoryAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.Products.GetAllWithCategoryAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<ProductDto>> BrowseAsync(ProductBrowseRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 12 : request.PageSize;

        var (items, total) = await _unitOfWork.Products.SearchAsync(
            request.Query,
            request.Brand,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.Sort,
            page,
            pageSize,
            cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.GetBrandsAsync(cancellationToken);
    }

    public async Task<ProductDto> CreateAsync(ProductDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.CategoryId is null)
        {
            throw new InvalidOperationException("Category is required.");
        }

        var skuBase = string.IsNullOrWhiteSpace(dto.Sku) ? GenerateSku(dto.Name) : GenerateSku(dto.Sku);
        var sku = await EnsureUniqueSkuAsync(skuBase, null, cancellationToken);

        var slugBase = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : GenerateSlug(dto.Slug);
        var slug = await EnsureUniqueSlugAsync(slugBase, null, cancellationToken);

        var entity = new Product
        {
            Name = dto.Name,
            Brand = dto.Brand,
            Sku = sku,
            Slug = slug,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl
        };

        await _unitOfWork.Products.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        dto.Id = entity.Id;
        dto.Sku = entity.Sku;
        dto.Slug = entity.Slug;
        return dto;
    }

    public async Task<bool> UpdateAsync(ProductDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Products.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (dto.CategoryId is null)
        {
            throw new InvalidOperationException("Category is required.");
        }

        var skuBase = string.IsNullOrWhiteSpace(dto.Sku) ? GenerateSku(dto.Name) : GenerateSku(dto.Sku);
        var sku = await EnsureUniqueSkuAsync(skuBase, dto.Id, cancellationToken);

        var slugBase = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : GenerateSlug(dto.Slug);
        var slug = await EnsureUniqueSlugAsync(slugBase, dto.Id, cancellationToken);

        entity.Name = dto.Name;
        entity.Brand = dto.Brand;
        entity.Sku = sku;
        entity.Slug = slug;
        entity.CategoryId = dto.CategoryId;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.StockQuantity = dto.StockQuantity;
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
            Brand = entity.Brand,
            Sku = entity.Sku,
            Slug = entity.Slug,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name,
            Description = entity.Description,
            Price = entity.Price,
            StockQuantity = entity.StockQuantity,
            ImageUrl = entity.ImageUrl
        };
    }

    private async Task<string> EnsureUniqueSkuAsync(string skuBase, int? excludeId, CancellationToken cancellationToken)
    {
        var sku = string.IsNullOrWhiteSpace(skuBase) ? "SKU" : skuBase;

        if (!await _unitOfWork.Products.SkuExistsAsync(sku, excludeId, cancellationToken))
        {
            return sku;
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{sku}-{suffix}";
            if (!await _unitOfWork.Products.SkuExistsAsync(candidate, excludeId, cancellationToken))
            {
                return candidate;
            }

            suffix++;
        }
    }

    private async Task<string> EnsureUniqueSlugAsync(string slugBase, int? excludeId, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(slugBase) ? "product" : slugBase;

        if (!await _unitOfWork.Products.SlugExistsAsync(slug, excludeId, cancellationToken))
        {
            return slug;
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{slug}-{suffix}";
            if (!await _unitOfWork.Products.SlugExistsAsync(candidate, excludeId, cancellationToken))
            {
                return candidate;
            }

            suffix++;
        }
    }

    private static string GenerateSku(string input)
    {
        var slug = GenerateSlug(input);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "ITEM";
        }

        return $"SS-{slug}".ToUpperInvariant();
    }

    private static string GenerateSlug(string input)
    {
        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        var cleaned = sb.ToString().Normalize(NormalizationForm.FormC);
        cleaned = Regex.Replace(cleaned, @"[^a-z0-9]+", "-");
        cleaned = Regex.Replace(cleaned, @"-+", "-").Trim('-');

        return cleaned;
    }
}
