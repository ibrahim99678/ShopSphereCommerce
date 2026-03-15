using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ProductVariantsController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductVariantService _variantService;

    public ProductVariantsController(IProductService productService, IProductVariantService variantService)
    {
        _productService = productService;
        _variantService = variantService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int productId, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Product = product;
        var variants = await _variantService.GetByProductIdAsync(productId, cancellationToken);
        return View(variants);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int productId, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Product = product;
        return View(new ProductVariantDto { ProductId = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductVariantDto model, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(model.ProductId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Product = product;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _variantService.CreateAsync(model, cancellationToken);
            return RedirectToAction(nameof(Index), new { productId = model.ProductId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var variant = await _variantService.GetByIdAsync(id, cancellationToken);
        if (variant is null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(variant.ProductId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Product = product;
        return View(variant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductVariantDto model, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(model.ProductId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        ViewBag.Product = product;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated = await _variantService.UpdateAsync(model, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index), new { productId = model.ProductId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int productId, CancellationToken cancellationToken)
    {
        await _variantService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { productId });
    }
}

