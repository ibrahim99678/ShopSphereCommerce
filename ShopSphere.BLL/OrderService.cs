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

    public async Task<OrderDto> CreateOrderAsync(
        string customerName,
        string mobile,
        string address,
        IReadOnlyList<CartItemDto> items,
        CancellationToken cancellationToken = default)
    {
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

        order.Status = (DomainOrderStatus)(int)status;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
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
            CustomerName = entity.CustomerName,
            Mobile = entity.Mobile,
            Address = entity.Address,
            Status = (OrderStatus)(int)entity.Status,
            Total = entity.Total,
            CreatedAtUtc = entity.CreatedAtUtc,
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

