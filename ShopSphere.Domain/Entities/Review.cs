namespace ShopSphere.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public required string UserId { get; set; }
    public required string ReviewerName { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public required string Comment { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}

