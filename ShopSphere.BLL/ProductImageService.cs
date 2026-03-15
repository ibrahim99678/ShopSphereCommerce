using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.BLL;

public class ProductImageService : IProductImageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    public ProductImageService(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<IReadOnlyList<ProductImageDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var images = await _unitOfWork.ProductImages.GetAllByProductIdAsync(productId, cancellationToken);
        return images.Select(MapToDto).ToList();
    }

    public async Task<ProductImageDto> UploadAsync(int productId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        if (file.Length <= 0)
        {
            throw new InvalidOperationException("Invalid file.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File is too large.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new InvalidOperationException("Unsupported image format.");
        }

        var relativeUrl = await SaveFileAsync(productId, file, ext, cancellationToken);
        var nextOrder = await _unitOfWork.ProductImages.GetNextSortOrderAsync(productId, cancellationToken);

        var existing = await _unitOfWork.ProductImages.GetAllByProductIdAsync(productId, cancellationToken);
        var isPrimary = existing.All(i => !i.IsPrimary);

        var entity = new ProductImage
        {
            ProductId = productId,
            ImageUrl = relativeUrl,
            SortOrder = nextOrder,
            IsPrimary = isPrimary
        };

        await _unitOfWork.ProductImages.AddAsync(entity, cancellationToken);

        if (isPrimary)
        {
            product.ImageUrl = relativeUrl;
            _unitOfWork.Products.Update(product);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ProductImages.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var product = await _unitOfWork.Products.GetByIdAsync(entity.ProductId, cancellationToken);
        _unitOfWork.ProductImages.Remove(entity);

        var all = await _unitOfWork.ProductImages.GetAllByProductIdAsync(entity.ProductId, cancellationToken);
        var remaining = all.Where(i => i.Id != entity.Id).OrderBy(i => i.SortOrder).ToList();

        if (entity.IsPrimary && product is not null)
        {
            var newPrimary = remaining.FirstOrDefault();
            product.ImageUrl = newPrimary?.ImageUrl;
            _unitOfWork.Products.Update(product);

            if (newPrimary is not null)
            {
                var trackedPrimary = await _unitOfWork.ProductImages.GetByIdAsync(newPrimary.Id, cancellationToken);
                if (trackedPrimary is not null)
                {
                    trackedPrimary.IsPrimary = true;
                    _unitOfWork.ProductImages.Update(trackedPrimary);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        TryDeleteFile(entity.ImageUrl);
        return true;
    }

    public async Task<bool> SetPrimaryAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ProductImages.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var product = await _unitOfWork.Products.GetByIdAsync(entity.ProductId, cancellationToken);
        if (product is null)
        {
            return false;
        }

        var images = await _unitOfWork.ProductImages.GetAllByProductIdAsync(entity.ProductId, cancellationToken);
        foreach (var img in images)
        {
            if (img.IsPrimary && img.Id != id)
            {
                var tracked = await _unitOfWork.ProductImages.GetByIdAsync(img.Id, cancellationToken);
                if (tracked is not null)
                {
                    tracked.IsPrimary = false;
                    _unitOfWork.ProductImages.Update(tracked);
                }
            }
        }

        entity.IsPrimary = true;
        _unitOfWork.ProductImages.Update(entity);

        product.ImageUrl = entity.ImageUrl;
        _unitOfWork.Products.Update(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MoveAsync(int id, int direction, CancellationToken cancellationToken = default)
    {
        if (direction != -1 && direction != 1)
        {
            return false;
        }

        var entity = await _unitOfWork.ProductImages.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var images = (await _unitOfWork.ProductImages.GetAllByProductIdAsync(entity.ProductId, cancellationToken))
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .ToList();

        var index = images.FindIndex(i => i.Id == id);
        if (index < 0)
        {
            return false;
        }

        var swapIndex = index + direction;
        if (swapIndex < 0 || swapIndex >= images.Count)
        {
            return false;
        }

        var other = images[swapIndex];

        var trackedOther = await _unitOfWork.ProductImages.GetByIdAsync(other.Id, cancellationToken);
        if (trackedOther is null)
        {
            return false;
        }

        var tmp = entity.SortOrder;
        entity.SortOrder = trackedOther.SortOrder;
        trackedOther.SortOrder = tmp;

        _unitOfWork.ProductImages.Update(entity);
        _unitOfWork.ProductImages.Update(trackedOther);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> SaveFileAsync(int productId, IFormFile file, string ext, CancellationToken cancellationToken)
    {
        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "products", productId.ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absolutePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/products/{productId}/{fileName}";
    }

    private void TryDeleteFile(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/", StringComparison.Ordinal))
            {
                return;
            }

            var relative = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullWebRoot = Path.GetFullPath(_environment.WebRootPath);
            var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relative));

            if (!fullPath.StartsWith(fullWebRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
        }
    }

    private static ProductImageDto MapToDto(ProductImage entity)
    {
        return new ProductImageDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ImageUrl = entity.ImageUrl,
            SortOrder = entity.SortOrder,
            IsPrimary = entity.IsPrimary
        };
    }
}

