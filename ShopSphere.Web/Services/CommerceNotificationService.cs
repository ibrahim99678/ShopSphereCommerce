using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Enums;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Identity;
using ShopSphere.Web.Identity;

namespace ShopSphere.Web.Services;

public class CommerceNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public CommerceNotificationService(IEmailSender emailSender, UserManager<ApplicationUser> userManager, INotificationService notificationService)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task NotifyOrderPlacedAsync(OrderDto order, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(order.UserId))
        {
            var user = await _userManager.FindByIdAsync(order.UserId);
            var email = user?.Email;
            if (!string.IsNullOrWhiteSpace(email))
            {
                var subject = $"Order Confirmation - {order.OrderNumber}";
                var body = BuildOrderEmailBody(order, "Your order has been placed successfully.");
                await TrySendAsync(email, subject, body);
            }

            await _notificationService.CreateAsync(
                order.UserId,
                NotificationType.OrderPlaced,
                "Order placed",
                $"Your order {order.OrderNumber} has been placed successfully.",
                order.Id,
                cancellationToken);
        }

        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        foreach (var admin in admins.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
        {
            await _notificationService.CreateAsync(
                admin.Id,
                NotificationType.AdminNewOrder,
                "New order",
                $"New order placed: {order.OrderNumber} ({order.Total:0.00}).",
                order.Id,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(admin.Email))
            {
                var subject = $"New Order - {order.OrderNumber}";
                var body = BuildOrderEmailBody(order, "A new order has been placed.");
                await TrySendAsync(admin.Email, subject, body);
            }
        }
    }

    public async Task NotifyOrderShippedAsync(OrderDto order, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(order.UserId))
        {
            return;
        }

        var user = await _userManager.FindByIdAsync(order.UserId);
        var email = user?.Email;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var subject = $"Order Shipped - {order.OrderNumber}";
            var body = BuildOrderEmailBody(order, "Your order has been shipped.");
            await TrySendAsync(email, subject, body);
        }

        await _notificationService.CreateAsync(
            order.UserId,
            NotificationType.OrderShipped,
            "Order shipped",
            $"Your order {order.OrderNumber} has been shipped.",
            order.Id,
            cancellationToken);
    }

    private static string BuildOrderEmailBody(OrderDto order, string intro)
    {
        static string E(string s) => WebUtility.HtmlEncode(s);

        var rows = string.Join("", order.Items.Select(i =>
            $"<tr><td>{E(i.ProductName)}</td><td style=\"text-align:right;\">{i.Price:0.00}</td><td style=\"text-align:right;\">{i.Quantity}</td><td style=\"text-align:right;\">{(i.Price * i.Quantity):0.00}</td></tr>"));

        return $@"
<div>
  <p>{E(intro)}</p>
  <p><strong>Order:</strong> {E(order.OrderNumber)}<br/>
     <strong>Total:</strong> {order.Total:0.00}<br/>
     <strong>Date:</strong> {order.CreatedAtUtc.ToLocalTime():g}</p>
  <table style=""width:100%; border-collapse: collapse;"" border=""1"" cellpadding=""6"">
    <thead>
      <tr>
        <th style=""text-align:left;"">Product</th>
        <th style=""text-align:right;"">Price</th>
        <th style=""text-align:right;"">Qty</th>
        <th style=""text-align:right;"">Total</th>
      </tr>
    </thead>
    <tbody>
      {rows}
    </tbody>
  </table>
</div>";
    }

    private async Task TrySendAsync(string email, string subject, string html)
    {
        try
        {
            await _emailSender.SendEmailAsync(email, subject, html);
        }
        catch
        {
        }
    }
}
