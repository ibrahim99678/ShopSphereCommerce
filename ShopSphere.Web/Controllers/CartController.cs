using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.BLL.Interfaces;

namespace ShopSphere.Web.Controllers;

public class CartController : Controller
{
    private const string CartSessionKey = "CartSessionId";

    private readonly IShoppingCartService _cartService;

    public CartController(IShoppingCartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetAsync(GetUserId(), GetOrCreateSessionId(), cancellationToken);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int? variantId, int quantity, string? returnUrl, CancellationToken cancellationToken)
    {
        try
        {
            await _cartService.AddAsync(GetUserId(), GetOrCreateSessionId(), productId, variantId, quantity, cancellationToken);
            TempData["Success"] = "Item added to cart.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int itemId, int quantity, CancellationToken cancellationToken)
    {
        try
        {
            await _cartService.UpdateQuantityAsync(GetUserId(), GetOrCreateSessionId(), itemId, quantity, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId, CancellationToken cancellationToken)
    {
        await _cartService.RemoveAsync(GetUserId(), GetOrCreateSessionId(), itemId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await _cartService.ClearAsync(GetUserId(), GetOrCreateSessionId(), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private string GetOrCreateSessionId()
    {
        var existing = HttpContext.Session.GetString(CartSessionKey);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var created = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString(CartSessionKey, created);
        return created;
    }
}
