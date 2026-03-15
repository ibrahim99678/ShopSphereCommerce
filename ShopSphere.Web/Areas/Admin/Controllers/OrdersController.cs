using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Contract.Enums;
using ShopSphere.Contract.Services;
using ShopSphere.Web.Services;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly CommerceNotificationService _commerceNotificationService;

    public OrdersController(IOrderService orderService, CommerceNotificationService commerceNotificationService)
    {
        _orderService = orderService;
        _commerceNotificationService = commerceNotificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _orderService.UpdateStatusAsync(id, status, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            if (status == OrderStatus.Shipped)
            {
                var order = await _orderService.GetByIdAsync(id, cancellationToken);
                if (order is not null)
                {
                    await _commerceNotificationService.NotifyOrderShippedAsync(order, cancellationToken);
                }
            }

            TempData["Success"] = "Order status updated.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

