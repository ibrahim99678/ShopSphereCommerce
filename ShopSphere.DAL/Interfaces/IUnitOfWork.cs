using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<Address> Addresses { get; }
    ICategoryRepository Categories { get; }
    IProductVariantRepository ProductVariants { get; }
    IProductImageRepository ProductImages { get; }
    IShoppingCartRepository ShoppingCarts { get; }
    IGenericRepository<Payment> Payments { get; }
    IReviewRepository Reviews { get; }
    INotificationRepository Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

