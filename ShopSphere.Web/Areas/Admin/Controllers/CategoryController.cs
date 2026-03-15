using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;

namespace ShopSphere.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        var items = BuildTreeItems(categories);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateParentsAsync(null, cancellationToken);
        return View(new CategoryDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentsAsync(null, cancellationToken);
            return View(model);
        }

        await _categoryService.CreateAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        await PopulateParentsAsync(id, cancellationToken);
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentsAsync(model.Id, cancellationToken);
            return View(model);
        }

        var updated = await _categoryService.UpdateAsync(model, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _categoryService.DeleteAsync(id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateParentsAsync(int? currentId, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        var items = BuildTreeItems(categories);

        var selectItems = items
            .Where(i => currentId == null || i.Id != currentId.Value)
            .Select(i => new SelectListItem
            {
                Value = i.Id.ToString(),
                Text = $"{new string('\u00A0', i.Depth * 4)}{i.Name}"
            })
            .ToList();

        selectItems.Insert(0, new SelectListItem { Value = "", Text = "(No parent)" });
        ViewBag.ParentCategories = selectItems;
    }

    private static List<CategoryTreeItem> BuildTreeItems(IReadOnlyList<CategoryDto> categories)
    {
        var roots = categories
            .Where(c => c.ParentCategoryId is null)
            .OrderBy(c => c.Name)
            .ToList();

        var byParent = categories
            .Where(c => c.ParentCategoryId is not null)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Name).ToList());

        var result = new List<CategoryTreeItem>();

        foreach (var root in roots)
        {
            result.Add(new CategoryTreeItem(root.Id, root.Name, root.Slug ?? string.Empty, root.ParentCategoryId, 0));
            AddChildren(root.Id, 1);
        }

        return result;

        void AddChildren(int parentId, int depth)
        {
            if (!byParent.TryGetValue(parentId, out var children))
            {
                return;
            }

            foreach (var child in children)
            {
                result.Add(new CategoryTreeItem(child.Id, child.Name, child.Slug ?? string.Empty, child.ParentCategoryId, depth));
                AddChildren(child.Id, depth + 1);
            }
        }
    }
}

public record CategoryTreeItem(int Id, string Name, string Slug, int? ParentCategoryId, int Depth);
