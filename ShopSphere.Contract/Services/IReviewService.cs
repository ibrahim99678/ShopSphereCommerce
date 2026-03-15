using ShopSphere.Contract.Dtos;

namespace ShopSphere.Contract.Services;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetApprovedByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDto>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<ReviewDto> CreateAsync(string userId, string reviewerName, CreateReviewDto review, CancellationToken cancellationToken = default);
    Task<bool> ApproveAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
