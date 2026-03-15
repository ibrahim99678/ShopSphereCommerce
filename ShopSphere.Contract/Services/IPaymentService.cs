using ShopSphere.Contract.Dtos;

namespace ShopSphere.Contract.Services;

public interface IPaymentService
{
    Task<PaymentDto?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreatePendingForOrderAsync(int orderId, string provider, string? userId, string? sessionId, CancellationToken cancellationToken = default);
    Task<bool> MarkPaidAsync(int orderId, string transactionId, string? gatewayReference, string? rawResponse, CancellationToken cancellationToken = default);
    Task<bool> MarkFailedAsync(int orderId, string? gatewayReference, string? rawResponse, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(int orderId, CancellationToken cancellationToken = default);
}

