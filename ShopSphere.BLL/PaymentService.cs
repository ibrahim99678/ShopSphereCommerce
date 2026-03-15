using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Enums;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;
using DomainPaymentStatus = ShopSphere.Domain.Enums.PaymentStatus;

namespace ShopSphere.BLL;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetAllAsync(cancellationToken);
        var payment = payments.FirstOrDefault(p => p.OrderId == orderId);
        return payment is null ? null : MapToDto(payment);
    }

    public async Task<PaymentDto> CreatePendingForOrderAsync(int orderId, string provider, string? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        if (!string.IsNullOrWhiteSpace(order.UserId) && !string.Equals(order.UserId, userId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Order does not belong to the current user.");
        }

        var existing = await GetByOrderIdAsync(orderId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var payment = new Payment
        {
            OrderId = orderId,
            Amount = order.Total,
            Provider = provider,
            Status = DomainPaymentStatus.Pending,
            SessionId = sessionId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(payment);
    }

    public async Task<bool> MarkPaidAsync(int orderId, string transactionId, string? gatewayReference, string? rawResponse, CancellationToken cancellationToken = default)
    {
        var payment = await GetEntityByOrderIdAsync(orderId, cancellationToken);
        if (payment is null)
        {
            return false;
        }

        if (payment.Status == DomainPaymentStatus.Paid)
        {
            return true;
        }

        payment.Status = DomainPaymentStatus.Paid;
        payment.TransactionId = transactionId;
        payment.GatewayReference = gatewayReference;
        payment.RawResponse = rawResponse;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkFailedAsync(int orderId, string? gatewayReference, string? rawResponse, CancellationToken cancellationToken = default)
    {
        var payment = await GetEntityByOrderIdAsync(orderId, cancellationToken);
        if (payment is null)
        {
            return false;
        }

        if (payment.Status == DomainPaymentStatus.Paid)
        {
            throw new InvalidOperationException("Payment is already marked as paid.");
        }

        payment.Status = DomainPaymentStatus.Failed;
        payment.GatewayReference = gatewayReference;
        payment.RawResponse = rawResponse;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CancelAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var payment = await GetEntityByOrderIdAsync(orderId, cancellationToken);
        if (payment is null)
        {
            return false;
        }

        if (payment.Status == DomainPaymentStatus.Paid)
        {
            throw new InvalidOperationException("Paid payments cannot be cancelled.");
        }

        payment.Status = DomainPaymentStatus.Cancelled;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Payment?> GetEntityByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        var payments = await _unitOfWork.Payments.GetAllAsync(cancellationToken);
        return payments.FirstOrDefault(p => p.OrderId == orderId);
    }

    private static PaymentDto MapToDto(Payment entity)
    {
        return new PaymentDto
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            Amount = entity.Amount,
            Provider = entity.Provider,
            Status = (PaymentStatus)(int)entity.Status,
            TransactionId = entity.TransactionId,
            SessionId = entity.SessionId,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
