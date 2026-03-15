using ShopSphere.DAL.Interfaces;
using ShopSphere.DAL.Repositories;
using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ShopSphereDbContext _dbContext;

    public UnitOfWork(ShopSphereDbContext dbContext)
    {
        _dbContext = dbContext;
        Products = new ProductRepository(_dbContext);
        Orders = new OrderRepository(_dbContext);
        OrderItems = new GenericRepository<OrderItem>(_dbContext);
        Addresses = new GenericRepository<Address>(_dbContext);
        Categories = new CategoryRepository(_dbContext);
        ProductVariants = new ProductVariantRepository(_dbContext);
        ProductImages = new ProductImageRepository(_dbContext);
        ShoppingCarts = new ShoppingCartRepository(_dbContext);
        Payments = new GenericRepository<Payment>(_dbContext);
        Reviews = new ReviewRepository(_dbContext);
        Notifications = new NotificationRepository(_dbContext);
    }

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public IGenericRepository<OrderItem> OrderItems { get; }
    public IGenericRepository<Address> Addresses { get; }
    public ICategoryRepository Categories { get; }
    public IProductVariantRepository ProductVariants { get; }
    public IProductImageRepository ProductImages { get; }
    public IShoppingCartRepository ShoppingCarts { get; }
    public IGenericRepository<Payment> Payments { get; }
    public IReviewRepository Reviews { get; }
    public INotificationRepository Notifications { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
