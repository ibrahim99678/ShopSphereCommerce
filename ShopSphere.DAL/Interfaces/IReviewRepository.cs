using ShopSphere.Domain.Entities;

namespace ShopSphere.DAL.Interfaces;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IReadOnlyList<Review>> GetApprovedByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int productId, string userId, CancellationToken cancellationToken = default);
}

