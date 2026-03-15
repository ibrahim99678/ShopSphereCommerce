using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.BLL;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.Categories.GetAllWithParentsAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CategoryDto dto, CancellationToken cancellationToken = default)
    {
        var slugBase = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : GenerateSlug(dto.Slug);
        var slug = await EnsureUniqueSlugAsync(slugBase, null, cancellationToken);

        var entity = new Category
        {
            Name = dto.Name,
            Slug = slug,
            ParentCategoryId = dto.ParentCategoryId
        };

        await _unitOfWork.Categories.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        dto.Id = entity.Id;
        dto.Slug = entity.Slug;
        return dto;
    }

    public async Task<bool> UpdateAsync(CategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Categories.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (dto.ParentCategoryId == dto.Id)
        {
            dto.ParentCategoryId = null;
        }

        var slugBase = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : GenerateSlug(dto.Slug);
        var slug = await EnsureUniqueSlugAsync(slugBase, dto.Id, cancellationToken);

        entity.Name = dto.Name;
        entity.Slug = slug;
        entity.ParentCategoryId = dto.ParentCategoryId;

        _unitOfWork.Categories.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Categories.GetByIdWithChildrenAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (entity.Children.Any())
        {
            throw new InvalidOperationException("Cannot delete a category that has child categories.");
        }

        _unitOfWork.Categories.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> EnsureUniqueSlugAsync(string slugBase, int? excludeId, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(slugBase) ? "category" : slugBase;

        if (!await _unitOfWork.Categories.SlugExistsAsync(slug, excludeId, cancellationToken))
        {
            return slug;
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{slug}-{suffix}";
            if (!await _unitOfWork.Categories.SlugExistsAsync(candidate, excludeId, cancellationToken))
            {
                return candidate;
            }

            suffix++;
        }
    }

    private static CategoryDto MapToDto(Category entity)
    {
        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            ParentCategoryId = entity.ParentCategoryId
        };
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

