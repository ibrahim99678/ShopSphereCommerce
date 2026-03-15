using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required]
    [StringLength(64)]
    public string Sku { get; set; } = string.Empty;

    [StringLength(32)]
    public string? Size { get; set; }

    [StringLength(32)]
    public string? Color { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PriceOverride { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}

