using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Contract.Dtos;

public class UserProfileDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(256)]
    public string? FullName { get; set; }

    [StringLength(64)]
    public string? PhoneNumber { get; set; }

    [StringLength(2048)]
    public string? ProfileImageUrl { get; set; }
}

