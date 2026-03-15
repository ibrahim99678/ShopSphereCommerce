namespace ShopSphere.Web.Models;

public class LowStockViewModel
{
    public int Threshold { get; init; }
    public IReadOnlyList<LowStockItemViewModel> Items { get; init; } = Array.Empty<LowStockItemViewModel>();
}

public class LowStockItemViewModel
{
    public required string Type { get; init; }
    public int ProductId { get; init; }
    public int? VariantId { get; init; }
    public required string ProductName { get; init; }
    public string? VariantLabel { get; init; }
    public int StockQuantity { get; init; }
}

