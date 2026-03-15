using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Web.Models;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class InventoryController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductVariantService _variantService;

    public InventoryController(IProductService productService, IProductVariantService variantService)
    {
        _productService = productService;
        _variantService = variantService;
    }

    [HttpGet]
    public async Task<IActionResult> LowStock(int threshold = 5, CancellationToken cancellationToken = default)
    {
        threshold = threshold < 0 ? 0 : threshold;

        var products = await _productService.GetAllAsync(cancellationToken);
        var items = new List<LowStockItemViewModel>();

        foreach (var p in products)
        {
            var variants = await _variantService.GetByProductIdAsync(p.Id, cancellationToken);
            var activeVariants = variants.Where(v => v.IsActive).ToList();
            if (activeVariants.Count > 0)
            {
                foreach (var v in activeVariants.Where(v => v.StockQuantity <= threshold))
                {
                    items.Add(new LowStockItemViewModel
                    {
                        Type = "Variant",
                        ProductId = p.Id,
                        VariantId = v.Id,
                        ProductName = p.Name,
                        VariantLabel = string.Join(" / ", new[] { v.Size, v.Color }.Where(x => !string.IsNullOrWhiteSpace(x))),
                        StockQuantity = v.StockQuantity
                    });
                }
            }
            else if (p.StockQuantity <= threshold)
            {
                items.Add(new LowStockItemViewModel
                {
                    Type = "Product",
                    ProductId = p.Id,
                    VariantId = null,
                    ProductName = p.Name,
                    VariantLabel = null,
                    StockQuantity = p.StockQuantity
                });
            }
        }

        var model = new LowStockViewModel
        {
            Threshold = threshold,
            Items = items.OrderBy(i => i.StockQuantity).ThenBy(i => i.ProductName).ToList()
        };

        return View(model);
    }
}

