using Microsoft.AspNetCore.Identity;

namespace ShopSphere.DAL.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? ProfileImageUrl { get; set; }
}
