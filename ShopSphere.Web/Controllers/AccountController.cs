using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Identity;

namespace ShopSphere.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserProfileService _userProfileService;
    private readonly IOrderService _orderService;
    private readonly INotificationService _notificationService;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        IUserProfileService userProfileService,
        IOrderService orderService,
        INotificationService notificationService,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _userProfileService = userProfileService;
        _orderService = orderService;
        _notificationService = notificationService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var profile = await _userProfileService.GetProfileAsync(userId, cancellationToken);
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UserProfileDto model, IFormFile? profileImage, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        model.Email = user.Email ?? model.Email;

        if (profileImage is not null && profileImage.Length > 0)
        {
            var ext = Path.GetExtension(profileImage.FileName);
            if (!AllowedImageExtensions.Contains(ext))
            {
                ModelState.AddModelError(nameof(profileImage), "Unsupported image format.");
            }
            else
            {
                var relativePath = await SaveProfileImageAsync(userId, profileImage, cancellationToken);
                model.ProfileImageUrl = relativePath;
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _userProfileService.UpdateProfileAsync(userId, model, cancellationToken);
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Password()
    {
        return Redirect("/Identity/Account/Manage/ChangePassword");
    }

    [HttpGet]
    public async Task<IActionResult> Orders(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var orders = await _orderService.GetByUserIdAsync(userId, cancellationToken);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var notifications = await _notificationService.GetForUserAsync(userId, 100, cancellationToken);
        ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(int id, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        await _notificationService.MarkReadAsync(userId, id, cancellationToken);
        return RedirectToAction(nameof(Notifications));
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(int id, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var orders = await _orderService.GetByUserIdAsync(userId, cancellationToken);
        var order = orders.FirstOrDefault(o => o.Id == id);
        if (order is null)
        {
            return Forbid();
        }

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> Addresses(CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var addresses = await _userProfileService.GetAddressesAsync(userId, cancellationToken);
        return View(addresses);
    }

    [HttpGet]
    public IActionResult CreateAddress()
    {
        return View(new AddressDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAddress(AddressDto model, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _userProfileService.CreateAddressAsync(userId, model, cancellationToken);
        return RedirectToAction(nameof(Addresses));
    }

    [HttpGet]
    public async Task<IActionResult> EditAddress(int id, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var addresses = await _userProfileService.GetAddressesAsync(userId, cancellationToken);
        var address = addresses.FirstOrDefault(a => a.Id == id);
        if (address is null)
        {
            return NotFound();
        }

        return View(address);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(AddressDto model, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _userProfileService.UpdateAddressAsync(userId, model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        await _userProfileService.DeleteAddressAsync(userId, id, cancellationToken);
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultAddress(int id, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        await _userProfileService.SetDefaultAddressAsync(userId, id, cancellationToken);
        return RedirectToAction(nameof(Addresses));
    }

    private async Task<string> SaveProfileImageAsync(string userId, IFormFile file, CancellationToken cancellationToken)
    {
        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "profiles", userId);
        Directory.CreateDirectory(uploadsRoot);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var absolutePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/profiles/{userId}/{fileName}";
    }
}
