namespace ShopSphere.Contract.Dtos;

public class ProductBrowseRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Sort { get; set; } = "newest";
    public string? Query { get; set; }
    public string? Brand { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
