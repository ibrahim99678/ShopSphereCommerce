using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Services;
using ShopSphere.Web.Models;
using ShopSphere.Web.Services;

namespace ShopSphere.Web.Controllers;

public class CheckoutController : Controller
{
    private const string CartSessionKey = "CartSessionId";
    private const string DefaultPaymentProvider = "MockPay";

    private readonly IShoppingCartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IUserProfileService _userProfileService;
    private readonly IPaymentService _paymentService;
    private readonly IProductService _productService;
    private readonly IProductVariantService _variantService;
    private readonly CommerceNotificationService _commerceNotificationService;

    public CheckoutController(IShoppingCartService cartService, IOrderService orderService, IUserProfileService userProfileService, IPaymentService paymentService, IProductService productService, IProductVariantService variantService, CommerceNotificationService commerceNotificationService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _userProfileService = userProfileService;
        _paymentService = paymentService;
        _productService = productService;
        _variantService = variantService;
        _commerceNotificationService = commerceNotificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var sessionId = GetOrCreateSessionId();

        var cart = await _cartService.GetAsync(userId, sessionId, cancellationToken);
        if (!cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        var addresses = Array.Empty<AddressDto>() as IReadOnlyList<AddressDto>;
        AddressDto? defaultAddress = null;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            addresses = await _userProfileService.GetAddressesAsync(userId, cancellationToken);
            defaultAddress = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();
        }

        var model = new CheckoutViewModel
        {
            Cart = cart,
            Addresses = addresses,
            SelectedAddressId = defaultAddress?.Id,
            CustomerName = defaultAddress?.FullName ?? string.Empty,
            Mobile = defaultAddress?.PhoneNumber ?? string.Empty,
            Address = defaultAddress is null ? string.Empty : BuildAddressString(defaultAddress),
            StockIssues = await BuildStockIssuesAsync(cart, cancellationToken)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var sessionId = GetOrCreateSessionId();

        var cart = await _cartService.GetAsync(userId, sessionId, cancellationToken);
        if (!cart.Items.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        var stockIssues = await BuildStockIssuesAsync(cart, cancellationToken);
        if (stockIssues.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "Some items are not available in the requested quantity.");
        }

        if (!ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                model.Addresses = await _userProfileService.GetAddressesAsync(userId, cancellationToken);
            }

            model.Cart = cart;
            model.StockIssues = stockIssues;
            return View("Index", model);
        }

        if (!string.IsNullOrWhiteSpace(userId) && model.SelectedAddressId is not null)
        {
            var addresses = await _userProfileService.GetAddressesAsync(userId, cancellationToken);
            var selected = addresses.FirstOrDefault(a => a.Id == model.SelectedAddressId.Value);
            if (selected is not null)
            {
                if (string.IsNullOrWhiteSpace(model.CustomerName))
                {
                    model.CustomerName = selected.FullName;
                }

                if (string.IsNullOrWhiteSpace(model.Mobile))
                {
                    model.Mobile = selected.PhoneNumber;
                }

                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    model.Address = BuildAddressString(selected);
                }
            }
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(
                model.CustomerName,
                model.Mobile,
                model.Address,
                cart.Items,
                userId,
                cancellationToken);

            await _paymentService.CreatePendingForOrderAsync(order.Id, DefaultPaymentProvider, userId, sessionId, cancellationToken);
            await _cartService.ClearAsync(userId, sessionId, cancellationToken);
            await _commerceNotificationService.NotifyOrderPlacedAsync(order, cancellationToken);
            return RedirectToAction("Pay", "Payment", new { orderId = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                model.Addresses = await _userProfileService.GetAddressesAsync(userId, cancellationToken);
            }

            model.Cart = cart;
            return View("Index", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
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

    private static string BuildAddressString(AddressDto address)
    {
        var parts = new List<string>
        {
            address.AddressLine1
        };

        if (!string.IsNullOrWhiteSpace(address.AddressLine2))
        {
            parts.Add(address.AddressLine2);
        }

        parts.Add($"{address.City}, {address.Country}{(string.IsNullOrWhiteSpace(address.PostalCode) ? "" : $" {address.PostalCode}")}");
        return string.Join(Environment.NewLine, parts);
    }

    private async Task<IReadOnlyList<string>> BuildStockIssuesAsync(ShoppingCartDto cart, CancellationToken cancellationToken)
    {
        var issues = new List<string>();

        foreach (var item in cart.Items)
        {
            if (item.ProductVariantId is not null)
            {
                var variant = await _variantService.GetByIdAsync(item.ProductVariantId.Value, cancellationToken);
                if (variant is null || !variant.IsActive)
                {
                    issues.Add($"{item.ProductName} is not available.");
                    continue;
                }

                if (variant.StockQuantity < item.Quantity)
                {
                    var label = string.IsNullOrWhiteSpace(item.VariantLabel) ? string.Empty : $" ({item.VariantLabel})";
                    issues.Add($"{item.ProductName}{label}: only {variant.StockQuantity} available.");
                }
            }
            else
            {
                var product = await _productService.GetByIdAsync(item.ProductId, cancellationToken);
                if (product is null)
                {
                    issues.Add($"{item.ProductName} is not available.");
                    continue;
                }

                if (product.StockQuantity < item.Quantity)
                {
                    issues.Add($"{item.ProductName}: only {product.StockQuantity} available.");
                }
            }
        }

        return issues;
    }
}
