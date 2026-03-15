using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;

namespace ShopSphere.BLL;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetApprovedByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var reviews = await _unitOfWork.Reviews.GetApprovedByProductIdAsync(productId, cancellationToken);
        return reviews.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ReviewDto>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var reviews = await _unitOfWork.Reviews.GetPendingAsync(cancellationToken);
        return reviews.Select(MapToDto).ToList();
    }

    public async Task<ReviewDto> CreateAsync(string userId, string reviewerName, CreateReviewDto review, CancellationToken cancellationToken = default)
    {
        if (review.Rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        if (string.IsNullOrWhiteSpace(review.Comment))
        {
            throw new InvalidOperationException("Review comment is required.");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(review.ProductId, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        var purchased = await _unitOfWork.Orders.HasDeliveredOrderWithProductAsync(userId, review.ProductId, cancellationToken);
        if (!purchased)
        {
            throw new InvalidOperationException("You can only review products you have purchased and received.");
        }

        var exists = await _unitOfWork.Reviews.ExistsAsync(review.ProductId, userId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("You have already reviewed this product.");
        }

        var entity = new Review
        {
            ProductId = review.ProductId,
            UserId = userId,
            ReviewerName = reviewerName,
            Rating = review.Rating,
            Title = string.IsNullOrWhiteSpace(review.Title) ? null : review.Title.Trim(),
            Comment = review.Comment.Trim(),
            IsApproved = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<bool> ApproveAsync(int id, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id, cancellationToken);
        if (review is null)
        {
            return false;
        }

        review.IsApproved = true;
        review.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id, cancellationToken);
        if (review is null)
        {
            return false;
        }

        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ReviewDto MapToDto(Review entity)
    {
        return new ReviewDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = entity.Product?.Name ?? string.Empty,
            UserId = entity.UserId,
            ReviewerName = entity.ReviewerName,
            Rating = entity.Rating,
            Title = entity.Title,
            Comment = entity.Comment,
            IsApproved = entity.IsApproved,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
