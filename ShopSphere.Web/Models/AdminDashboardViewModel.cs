using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Models;

public class AdminDashboardViewModel
{
    public int TotalOrders { get; init; }
    public decimal TotalSales { get; init; }
    public int TotalCustomers { get; init; }
    public IReadOnlyList<OrderDto> RecentOrders { get; init; } = Array.Empty<OrderDto>();
}

