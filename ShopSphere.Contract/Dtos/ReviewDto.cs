using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class ReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(128)]
    public string? Title { get; set; }

    [Required]
    [StringLength(2000)]
    public string Comment { get; set; } = string.Empty;

    public bool IsApproved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
