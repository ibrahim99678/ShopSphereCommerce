using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.DAL.Identity;
using ShopSphere.Web.Identity;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = _userManager.Users.OrderBy(u => u.Email).ToList();
        var rows = new List<UserRoleRow>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            rows.Add(new UserRoleRow(
                user.Id,
                user.Email ?? user.UserName ?? string.Empty,
                user.FullName,
                user.EmailConfirmed,
                roles.Contains(Roles.Admin),
                roles.Contains(Roles.Customer)));
        }

        var model = new UsersIndexViewModel(rows);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoles(UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        if (!await _roleManager.RoleExistsAsync(Roles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }

        if (!await _roleManager.RoleExistsAsync(Roles.Customer))
        {
            await _roleManager.CreateAsync(new IdentityRole(Roles.Customer));
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound();
        }

        await ApplyRoleAsync(user, Roles.Admin, request.IsAdmin);
        await ApplyRoleAsync(user, Roles.Customer, request.IsCustomer);

        return RedirectToAction(nameof(Index));
    }

    private async Task ApplyRoleAsync(ApplicationUser user, string role, bool shouldHaveRole)
    {
        var hasRole = await _userManager.IsInRoleAsync(user, role);

        if (shouldHaveRole && !hasRole)
        {
            await _userManager.AddToRoleAsync(user, role);
            return;
        }

        if (!shouldHaveRole && hasRole)
        {
            await _userManager.RemoveFromRoleAsync(user, role);
        }
    }
}

public record UsersIndexViewModel(IReadOnlyList<UserRoleRow> Users);

public record UserRoleRow(
    string Id,
    string Email,
    string? FullName,
    bool EmailConfirmed,
    bool IsAdmin,
    bool IsCustomer);

public class UpdateUserRolesRequest
{
    public string UserId { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsCustomer { get; set; }
}

