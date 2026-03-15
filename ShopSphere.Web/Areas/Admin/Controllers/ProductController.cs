using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IProductImageService _productImageService;

    public ProductController(IProductService productService, ICategoryService categoryService, IProductImageService productImageService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _productImageService = productImageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        if (!categories.Any())
        {
            TempData["Error"] = "Please create a category before creating products.";
            return RedirectToAction("Create", "Category", new { area = "Admin" });
        }

        await PopulateCategoriesAsync(null, cancellationToken);
        return View(new ProductDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductDto model, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
            return View(model);
        }

        try
        {
            if (imageFile is not null && imageFile.Length > 0)
            {
                model.ImageUrl = null;
            }

            var created = await _productService.CreateAsync(model, cancellationToken);

            if (imageFile is not null && imageFile.Length > 0)
            {
                try
                {
                    await _productImageService.UploadAsync(created.Id, imageFile, cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return RedirectToAction(nameof(Edit), new { id = created.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync(product.CategoryId, cancellationToken);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
            return View(model);
        }

        try
        {
            var updated = await _productService.UpdateAsync(model, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCategoriesAsync(model.CategoryId, cancellationToken);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(int? selectedId, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);

        var roots = categories
            .Where(c => c.ParentCategoryId is null)
            .OrderBy(c => c.Name)
            .ToList();

        var byParent = categories
            .Where(c => c.ParentCategoryId is not null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name).ToList());

        var options = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = "-- Select category --", Selected = selectedId is null }
        };
        foreach (var root in roots)
        {
            options.Add(new SelectListItem { Value = root.Id.ToString(), Text = root.Name, Selected = selectedId == root.Id });
            AddChildren(root.Id, 1);
        }

        ViewBag.Categories = options;

        void AddChildren(int parentId, int depth)
        {
            if (!byParent.TryGetValue(parentId, out var children))
            {
                return;
            }

            foreach (var child in children)
            {
                options.Add(new SelectListItem
                {
                    Value = child.Id.ToString(),
                    Text = $"{new string('\u00A0', depth * 4)}{child.Name}",
                    Selected = selectedId == child.Id
                });

                AddChildren(child.Id, depth + 1);
            }
        }
    }
}
