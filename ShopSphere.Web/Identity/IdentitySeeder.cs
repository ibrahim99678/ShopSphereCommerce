using Microsoft.AspNetCore.Identity;
using ShopSphere.DAL.Identity;

namespace ShopSphere.Web.Identity;

public static class IdentitySeeder
{
    private const string DefaultAdminEmail = "admin@shopsphere.com";
    private const string DefaultAdminPassword = "Admin@123";

    public static async Task SeedIdentityAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var configuration = scope.ServiceProvider.GetService<IConfiguration>();

        await EnsureRoleAsync(roleManager, Roles.Admin);
        await EnsureRoleAsync(roleManager, Roles.Customer);

        var adminEmail = configuration?["Seed:AdminEmail"];
        var adminPassword = configuration?["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) && environment.IsDevelopment())
        {
            adminEmail = DefaultAdminEmail;
        }

        if (string.IsNullOrWhiteSpace(adminPassword) && environment.IsDevelopment())
        {
            adminPassword = DefaultAdminPassword;
        }

        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                if (string.IsNullOrWhiteSpace(adminPassword))
                {
                    return;
                }

                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    return;
                }
            }

            if (!admin.EmailConfirmed)
            {
                admin.EmailConfirmed = true;
                await userManager.UpdateAsync(admin);
            }

            if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
