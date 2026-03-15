using Microsoft.AspNetCore.Mvc;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Services;
using ShopSphere.Web.Models;

namespace ShopSphere.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductVariantService _variantService;
    private readonly IProductImageService _imageService;
    private readonly ICategoryService _categoryService;
    private readonly IReviewService _reviewService;

    public ProductsController(IProductService productService, IProductVariantService variantService, IProductImageService imageService, ICategoryService categoryService, IReviewService reviewService)
    {
        _productService = productService;
        _variantService = variantService;
        _imageService = imageService;
        _categoryService = categoryService;
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductBrowseRequest request, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        var brands = await _productService.GetBrandsAsync(cancellationToken);
        var results = await _productService.BrowseAsync(request, cancellationToken);

        return View(new ProductBrowseViewModel
        {
            Categories = categories,
            Brands = brands,
            Results = results,
            Request = request
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var variants = await _variantService.GetByProductIdAsync(id, cancellationToken);
        var images = await _imageService.GetByProductIdAsync(id, cancellationToken);
        var reviews = await _reviewService.GetApprovedByProductIdAsync(id, cancellationToken);
        var model = new ProductDetailsViewModel
        {
            Product = product,
            Variants = variants,
            Images = images,
            Reviews = reviews,
            ReviewCount = reviews.Count,
            AverageRating = reviews.Count == 0 ? 0m : (decimal)reviews.Average(r => r.Rating)
        };

        return View(model);
    }
}
