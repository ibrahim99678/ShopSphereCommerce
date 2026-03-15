using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class ProductDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(128)]
    public string? Brand { get; set; }

    [StringLength(64)]
    public string? Sku { get; set; }

    [StringLength(200)]
    public string? Slug { get; set; }

    [Required]
    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [StringLength(2048)]
    public string? ImageUrl { get; set; }
}
