using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Identity;

namespace ShopSphere.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IReviewService _reviewService;

    public ReviewsController(UserManager<ApplicationUser> userManager, IReviewService reviewService)
    {
        _userManager = userManager;
        _reviewService = reviewService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewDto model, CancellationToken cancellationToken)
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

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check your review input.";
            return RedirectToAction("Details", "Products", new { id = model.ProductId });
        }

        var reviewerName = user.FullName ?? user.Email ?? user.UserName ?? "Customer";

        try
        {
            await _reviewService.CreateAsync(userId, reviewerName, model, cancellationToken);
            TempData["Success"] = "Review submitted for moderation.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", "Products", new { id = model.ProductId });
    }
}

