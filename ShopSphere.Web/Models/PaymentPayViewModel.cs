using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Models;

public class PaymentPayViewModel
{
    public required OrderDto Order { get; init; }
    public required PaymentDto Payment { get; init; }
}

