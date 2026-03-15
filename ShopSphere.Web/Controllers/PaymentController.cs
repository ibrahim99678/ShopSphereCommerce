using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Contract.Services;
using ShopSphere.Web.Models;

namespace ShopSphere.Web.Controllers;

public class PaymentController : Controller
{
    private const string CartSessionKey = "CartSessionId";
    private const string DefaultProvider = "MockPay";

    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;

    public PaymentController(IOrderService orderService, IPaymentService paymentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> Pay(int orderId, CancellationToken cancellationToken)
    {
        var sessionId = GetOrCreateSessionId();
        var userId = GetUserId();

        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!await CanAccessOrderAsync(order, userId, sessionId, cancellationToken))
        {
            return Forbid();
        }

        var payment = await _paymentService.CreatePendingForOrderAsync(orderId, DefaultProvider, userId, sessionId, cancellationToken);
        return View(new PaymentPayViewModel
        {
            Order = order,
            Payment = payment
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int orderId, CancellationToken cancellationToken)
    {
        var sessionId = GetOrCreateSessionId();
        var userId = GetUserId();

        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!await CanAccessOrderAsync(order, userId, sessionId, cancellationToken))
        {
            return Forbid();
        }

        var tx = $"TX-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32].ToUpperInvariant();
        await _paymentService.MarkPaidAsync(orderId, tx, gatewayReference: "MOCK_SUCCESS", rawResponse: "OK", cancellationToken);
        TempData["Success"] = "Payment successful.";
        return RedirectToAction("Success", "Checkout", new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fail(int orderId, CancellationToken cancellationToken)
    {
        var sessionId = GetOrCreateSessionId();
        var userId = GetUserId();

        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!await CanAccessOrderAsync(order, userId, sessionId, cancellationToken))
        {
            return Forbid();
        }

        await _paymentService.MarkFailedAsync(orderId, gatewayReference: "MOCK_FAILED", rawResponse: "FAILED", cancellationToken);
        TempData["Error"] = "Payment failed.";
        return RedirectToAction(nameof(Pay), new { orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int orderId, CancellationToken cancellationToken)
    {
        var sessionId = GetOrCreateSessionId();
        var userId = GetUserId();

        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (!await CanAccessOrderAsync(order, userId, sessionId, cancellationToken))
        {
            return Forbid();
        }

        await _paymentService.CancelAsync(orderId, cancellationToken);
        return RedirectToAction("Success", "Checkout", new { id = orderId });
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

    private async Task<bool> CanAccessOrderAsync(ShopSphere.Contract.Dtos.OrderDto order, string? userId, string sessionId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(order.UserId))
        {
            return !string.IsNullOrWhiteSpace(userId) && string.Equals(userId, order.UserId, StringComparison.Ordinal);
        }

        var payment = await _paymentService.GetByOrderIdAsync(order.Id, cancellationToken);
        return payment is not null && string.Equals(payment.SessionId, sessionId, StringComparison.Ordinal);
    }
}
