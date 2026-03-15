using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class AddressDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(512)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(512)]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(128)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Country { get; set; } = string.Empty;

    [StringLength(32)]
    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }
}

