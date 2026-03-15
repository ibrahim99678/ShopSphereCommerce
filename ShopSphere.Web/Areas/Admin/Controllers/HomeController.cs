using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Contract.Enums;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Identity;
using ShopSphere.Web.Identity;
using ShopSphere.Web.Models;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class HomeController : Controller
{
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(IOrderService orderService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(cancellationToken);
        var customers = await _userManager.GetUsersInRoleAsync(Roles.Customer);

        var totalSales = orders
            .Where(o => o.Payment is not null && o.Payment.Status == PaymentStatus.Paid)
            .Sum(o => o.Total);

        var model = new AdminDashboardViewModel
        {
            TotalOrders = orders.Count,
            TotalSales = totalSales,
            TotalCustomers = customers.Count,
            RecentOrders = orders.OrderByDescending(o => o.CreatedAtUtc).Take(5).ToList()
        };

        return View(model);
    }
}

