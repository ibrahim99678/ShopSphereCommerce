using ShopSphere.Contract.Dtos;
using ShopSphere.Contract.Enums;
using ShopSphere.Contract.Services;
using ShopSphere.DAL.Interfaces;
using ShopSphere.Domain.Entities;
using DomainOrderStatus = ShopSphere.Domain.Enums.OrderStatus;

namespace ShopSphere.BLL;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id, cancellationToken);
        return order is null ? null : MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllWithItemsAsync(cancellationToken);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllByUserIdWithItemsAsync(userId, cancellationToken);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> CreateOrderAsync(
        string customerName,
        string mobile,
        string address,
        IReadOnlyList<CartItemDto> items,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        foreach (var item in items)
        {
            if (item.Quantity < 1)
            {
                throw new InvalidOperationException("Invalid item quantity.");
            }
        }

        foreach (var item in items.Where(i => i.ProductVariantId is null))
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                throw new InvalidOperationException("One or more products are not available.");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock for one or more items.");
            }

            product.StockQuantity -= item.Quantity;
        }

        foreach (var item in items.Where(i => i.ProductVariantId is not null))
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.ProductVariantId!.Value, cancellationToken);
            if (variant is null || variant.ProductId != item.ProductId || !variant.IsActive)
            {
                throw new InvalidOperationException("One or more product variants are not available.");
            }

            if (variant.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock for one or more items.");
            }

            variant.StockQuantity -= item.Quantity;
        }

        var orderItems = items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Price = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList();

        var total = orderItems.Sum(i => i.Price * i.Quantity);

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            CustomerName = customerName,
            Mobile = mobile,
            Address = address,
            Status = DomainOrderStatus.Pending,
            Total = total,
            OrderItems = orderItems
        };

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    public async Task<bool> UpdateStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return false;
        }

        var current = (OrderStatus)(int)order.Status;
        if (!IsAllowedTransition(current, status))
        {
            throw new InvalidOperationException($"Invalid status transition: {current} → {status}.");
        }

        order.Status = (DomainOrderStatus)(int)status;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool IsAllowedTransition(OrderStatus current, OrderStatus next)
    {
        if (current == next)
        {
            return true;
        }

        return current switch
        {
            OrderStatus.Pending => next is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => next is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => next is OrderStatus.Delivered,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..25].ToUpperInvariant();
    }

    private static OrderDto MapToDto(Order entity)
    {
        return new OrderDto
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            UserId = entity.UserId,
            CustomerName = entity.CustomerName,
            Mobile = entity.Mobile,
            Address = entity.Address,
            Status = (OrderStatus)(int)entity.Status,
            Total = entity.Total,
            CreatedAtUtc = entity.CreatedAtUtc,
            Payment = entity.Payment is null ? null : new PaymentDto
            {
                Id = entity.Payment.Id,
                OrderId = entity.Payment.OrderId,
                Amount = entity.Payment.Amount,
                Provider = entity.Payment.Provider,
                Status = (PaymentStatus)(int)entity.Payment.Status,
                TransactionId = entity.Payment.TransactionId,
                SessionId = entity.Payment.SessionId,
                CreatedAtUtc = entity.Payment.CreatedAtUtc
            },
            Items = entity.OrderItems.Select(i => new OrderItemDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
