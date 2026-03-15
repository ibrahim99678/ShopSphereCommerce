using System.ComponentModel.DataAnnotations;
using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Models;

public class CheckoutViewModel
{
    public ShoppingCartDto Cart { get; set; } = new();
    public IReadOnlyList<AddressDto> Addresses { get; set; } = Array.Empty<AddressDto>();
    public int? SelectedAddressId { get; set; }
    public IReadOnlyList<string> StockIssues { get; set; } = Array.Empty<string>();
    public bool CanPlaceOrder => StockIssues.Count == 0;

    [Required]
    [StringLength(256)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Address { get; set; } = string.Empty;
}
