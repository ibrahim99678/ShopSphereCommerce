using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Models;

public class ProductBrowseViewModel
{
    public required PagedResult<ProductDto> Results { get; init; }
    public required IReadOnlyList<CategoryDto> Categories { get; init; }
    public required IReadOnlyList<string> Brands { get; init; }
    public required ProductBrowseRequest Request { get; init; }
}
