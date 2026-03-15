using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class ProductImageDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required]
    [StringLength(2048)]
    public string ImageUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

