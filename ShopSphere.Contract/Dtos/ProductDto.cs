using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class ProductDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [StringLength(2048)]
    public string? ImageUrl { get; set; }
}

