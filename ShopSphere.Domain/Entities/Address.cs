namespace ShopSphere.Domain.Entities;

public class Address
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

