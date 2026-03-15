using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class CreateReviewDto
{
    public int ProductId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(128)]
    public string? Title { get; set; }

    [Required]
    [StringLength(2000)]
    public string Comment { get; set; } = string.Empty;
}

