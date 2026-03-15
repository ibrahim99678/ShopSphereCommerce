using ShopSphere.BLL.Interfaces;
using ShopSphere.Contract.Dtos;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.BLL;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingCartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ShoppingCartDto> GetAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, sessionId, createIfMissing: false, cancellationToken);
        return cart is null ? new ShoppingCartDto() : await MapToDtoAsync(cart, cancellationToken);
    }

    public async Task AddAsync(string? userId, string? sessionId, int productId, int? variantId, int quantity, CancellationToken cancellationToken = default)
    {
        quantity = quantity < 1 ? 1 : quantity;

        var cart = await GetCartAsync(userId, sessionId, createIfMissing: true, cancellationToken);
        if (cart is null)
        {
            throw new InvalidOperationException("Cart not found.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        ProductVariant? variant = null;
        if (variantId is not null)
        {
            variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId.Value, cancellationToken);
            if (variant is null || variant.ProductId != productId || !variant.IsActive)
            {
                throw new InvalidOperationException("Variant not found.");
            }
        }

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
        if (existing is not null)
        {
            var desired = existing.Quantity + quantity;
            EnsureStockAvailable(product, variant, desired);
            existing.Quantity = desired;
            cart.UpdatedAtUtc = DateTime.UtcNow;
            _unitOfWork.ShoppingCarts.Update(cart);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var unitPrice = variant?.PriceOverride ?? product.Price;
        EnsureStockAvailable(product, variant, quantity);

        cart.Items.Add(new CartItem
        {
            ProductId = productId,
            ProductVariantId = variantId,
            UnitPrice = unitPrice,
            Quantity = quantity
        });

        cart.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.ShoppingCarts.Update(cart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateQuantityAsync(string? userId, string? sessionId, int itemId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, sessionId, createIfMissing: false, cancellationToken);
        if (cart is null)
        {
            return false;
        }

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            return false;
        }

        if (quantity < 1)
        {
            cart.Items.Remove(item);
        }
        else
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            ProductVariant? variant = null;
            if (item.ProductVariantId is not null)
            {
                variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.ProductVariantId.Value, cancellationToken);
                if (variant is null || variant.ProductId != item.ProductId || !variant.IsActive)
                {
                    throw new InvalidOperationException("Variant not found.");
                }
            }

            EnsureStockAvailable(product, variant, quantity);
            item.Quantity = quantity;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.ShoppingCarts.Update(cart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveAsync(string? userId, string? sessionId, int itemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, sessionId, createIfMissing: false, cancellationToken);
        if (cart is null)
        {
            return false;
        }

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            return false;
        }

        cart.Items.Remove(item);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.ShoppingCarts.Update(cart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ClearAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, sessionId, createIfMissing: false, cancellationToken);
        if (cart is null)
        {
            return;
        }

        cart.Items.Clear();
        cart.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.ShoppingCarts.Update(cart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ShoppingCart?> GetCartAsync(string? userId, string? sessionId, bool createIfMissing, CancellationToken cancellationToken)
    {
        ShoppingCart? cart = null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(userId, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(sessionId))
        {
            cart = await _unitOfWork.ShoppingCarts.GetBySessionIdAsync(sessionId, cancellationToken);
        }

        if (cart is not null || !createIfMissing)
        {
            return cart;
        }

        if (string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        cart = new ShoppingCart
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
            SessionId = string.IsNullOrWhiteSpace(sessionId) ? null : sessionId
        };

        await _unitOfWork.ShoppingCarts.AddAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await _unitOfWork.ShoppingCarts.GetByIdWithItemsAsync(cart.Id, cancellationToken);
    }

    private async Task<ShoppingCartDto> MapToDtoAsync(ShoppingCart cart, CancellationToken cancellationToken)
    {
        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var variantIds = cart.Items.Where(i => i.ProductVariantId is not null).Select(i => i.ProductVariantId!.Value).Distinct().ToList();

        var products = await _unitOfWork.Products.GetByIdsAsync(productIds, cancellationToken);
        var variants = await _unitOfWork.ProductVariants.GetByIdsAsync(variantIds, cancellationToken);

        var productById = products.ToDictionary(p => p.Id);
        var variantById = variants.ToDictionary(v => v.Id);

        return new ShoppingCartDto
        {
            Id = cart.Id,
            Items = cart.Items
                .OrderByDescending(i => i.CreatedAtUtc)
                .Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = productById.TryGetValue(i.ProductId, out var p) ? p.Name : string.Empty,
                    VariantLabel = i.ProductVariantId is null ? null : (variantById.TryGetValue(i.ProductVariantId.Value, out var v) ? BuildVariantLabel(v) : null),
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    ImageUrl = productById.TryGetValue(i.ProductId, out var p2) ? p2.ImageUrl : null
                })
                .ToList()
        };
    }

    private static string? BuildVariantLabel(ProductVariant? variant)
    {
        if (variant is null)
        {
            return null;
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(variant.Size))
        {
            parts.Add(variant.Size);
        }

        if (!string.IsNullOrWhiteSpace(variant.Color))
        {
            parts.Add(variant.Color);
        }

        return parts.Count == 0 ? null : string.Join(" / ", parts);
    }

    private static void EnsureStockAvailable(Product product, ProductVariant? variant, int desiredQuantity)
    {
        if (desiredQuantity < 1)
        {
            throw new InvalidOperationException("Invalid quantity.");
        }

        var stock = variant?.StockQuantity ?? product.StockQuantity;
        if (stock <= 0)
        {
            throw new InvalidOperationException("This item is out of stock.");
        }

        if (desiredQuantity > stock)
        {
            throw new InvalidOperationException($"Only {stock} item(s) available in stock.");
        }
    }
}
