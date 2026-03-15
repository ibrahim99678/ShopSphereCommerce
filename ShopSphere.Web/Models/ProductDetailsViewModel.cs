using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Models;

public class ProductDetailsViewModel
{
    public required ProductDto Product { get; init; }
    public required IReadOnlyList<ProductVariantDto> Variants { get; init; }
    public required IReadOnlyList<ProductImageDto> Images { get; init; }
    public IReadOnlyList<ReviewDto> Reviews { get; init; } = Array.Empty<ReviewDto>();
    public decimal AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
