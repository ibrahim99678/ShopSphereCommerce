using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class CategoryDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Slug { get; set; }

    public int? ParentCategoryId { get; set; }
}

