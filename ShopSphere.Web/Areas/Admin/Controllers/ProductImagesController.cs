using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.BLL.Interfaces;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ProductImagesController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductImageService _imageService;

    public ProductImagesController(IProductService productService, IProductImageService imageService)
    {
        _productService = productService;
        _imageService = imageService;
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
        var images = await _imageService.GetByProductIdAsync(productId, cancellationToken);
        return View(images);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int productId, IFormFile file, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        try
        {
            await _imageService.UploadAsync(productId, file, cancellationToken);
            return RedirectToAction(nameof(Index), new { productId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index), new { productId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimary(int id, int productId, CancellationToken cancellationToken)
    {
        await _imageService.SetPrimaryAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(int id, int productId, int direction, CancellationToken cancellationToken)
    {
        await _imageService.MoveAsync(id, direction, cancellationToken);
        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int productId, CancellationToken cancellationToken)
    {
        await _imageService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index), new { productId });
    }
}

