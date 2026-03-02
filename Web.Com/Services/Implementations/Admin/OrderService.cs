using Microsoft.AspNetCore.SignalR;
using Web.Com.DTOs.Admin;
using Web.Com.DTOs.User;
using Web.Com.Entities;
using Web.Com.Hubs;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Repositories.Interfaces.User;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Services.Implementations.Admin;

public class OrderService : IOrderService
{
  private readonly IOrderRepository _orderRepository;
  private readonly ICartRepository _cartRepository;
  private readonly IHubContext<NotificationHub> _hubContext;

  public OrderService(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IHubContext<NotificationHub> hubContext
  )
  {
    _orderRepository = orderRepository;
    _cartRepository = cartRepository;
    _hubContext = hubContext;
  }

  public async Task<IEnumerable<AdminOrderDto>> GetOrdersAsync(string? status)
  {
    OrderStatus? orderStatus = null;
    if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
    {
      orderStatus = parsedStatus;
    }

    var orders = await _orderRepository.GetAllAsync(orderStatus);
    return orders.Select(o => new AdminOrderDto
    {
      Id = o.Id,
      CustomerName = $"{o.User?.FirstName} {o.User?.LastName}",
      CustomerEmail = o.User?.Email ?? string.Empty,
      OrderDate = o.OrderDate,
      TotalAmount = o.TotalAmount,
      Status = o.Status.ToString(),
      PaymentMethod = o.PaymentMethod,
      PaymentStatus = o.PaymentStatus,
      TrackingNumber = o.TrackingNumber,
      ShippingAddress = o.ShippingAddress,
      PhoneNumber = o.PhoneNumber,
      ItemCount = o.OrderItems.Count,
    });
  }

  public async Task<bool> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto)
  {
    var order = await _orderRepository.GetByIdAsync(id);
    if (order == null)
      return false;

    if (!Enum.TryParse<OrderStatus>(dto.Status, out var newStatus))
      throw new ArgumentException("Invalid status");

    var previousStatus = order.Status;
    order.Status = newStatus;

    if (!string.IsNullOrEmpty(dto.TrackingNumber))
      order.TrackingNumber = dto.TrackingNumber;

    if (
      previousStatus == OrderStatus.Pending
      && newStatus == OrderStatus.Confirmed
      && order.PaymentStatus == "Paid"
    )
    {
      foreach (var item in order.OrderItems)
      {
        if (item.Product != null)
        {
          item.Product.Stock -= item.Quantity;
        }
      }
    }

    await _orderRepository.UpdateAsync(order);

    var notification = new Notification
    {
      UserId = order.UserId,
      Title = $"Order {newStatus}",
      Message = newStatus switch
      {
        OrderStatus.Confirmed => $"Your order #{order.Id} has been confirmed.",
        OrderStatus.Shipped =>
          $"Your order #{order.Id} has been shipped. Tracking: {order.TrackingNumber}",
        OrderStatus.Delivered => $"Your order #{order.Id} has been delivered.",
        OrderStatus.Cancelled => $"Your order #{order.Id} has been cancelled.",
        _ => $"Your order #{order.Id} status is {newStatus}.",
      },
    };

    await _orderRepository.AddNotificationAsync(notification);

    // Push real-time notification via SignalR
    await _hubContext
      .Clients.User(order.UserId)
      .SendAsync(
        "ReceiveNotification",
        new
        {
          id = notification.Id,
          title = notification.Title,
          message = notification.Message,
          isRead = notification.IsRead,
          createdAt = notification.CreatedAt,
        }
      );

    return true;
  }

  // User Methods
  public async Task<OrderResponseDto> CreateOrderAsync(string userId, CreateOrderDto dto)
  {
    var cartItems = (await _cartRepository.GetCartItemsAsync(userId)).ToList();
    if (!cartItems.Any())
      throw new InvalidOperationException("Cart is empty");

    foreach (var item in cartItems)
    {
      if (item.Product.Stock < item.Quantity)
        throw new InvalidOperationException($"Insufficient stock for {item.Product.Name}");
    }

    decimal total = cartItems.Sum(c => (c.Product.DiscountPrice ?? c.Product.Price) * c.Quantity);

    var order = new Order
    {
      UserId = userId,
      TotalAmount = total,
      PaymentMethod = dto.PaymentMethod,
      PaymentStatus = dto.PaymentMethod == "COD" ? "COD" : "Pending",
      ShippingAddress = dto.ShippingAddress,
      PhoneNumber = dto.PhoneNumber,
      Status = OrderStatus.Pending,
    };

    foreach (var cartItem in cartItems)
    {
      order.OrderItems.Add(
        new OrderItem
        {
          ProductId = cartItem.ProductId,
          Quantity = cartItem.Quantity,
          PriceAtPurchase = cartItem.Product.DiscountPrice ?? cartItem.Product.Price,
        }
      );

      if (dto.PaymentMethod == "COD")
        cartItem.Product.Stock -= cartItem.Quantity;
    }

    await _orderRepository.CreateAsync(order);

    if (dto.PaymentMethod == "COD")
      await _cartRepository.ClearCartAsync(userId);

    var notification = new Notification
    {
      UserId = userId,
      Title = "Order Created",
      Message = $"Your order #{order.Id} has been created successfully.",
    };

    await _orderRepository.AddNotificationAsync(notification);

    // Push real-time notification via SignalR
    await _hubContext
      .Clients.User(userId)
      .SendAsync(
        "ReceiveNotification",
        new
        {
          id = notification.Id,
          title = notification.Title,
          message = notification.Message,
          isRead = notification.IsRead,
          createdAt = notification.CreatedAt,
        }
      );

    return new OrderResponseDto
    {
      OrderId = order.Id,
      Message = "Order created successfully",
    };
  }

  public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(string userId)
  {
    var orders = await _orderRepository.GetByUserIdAsync(userId);
    return orders.Select(o => new OrderDto
    {
      Id = o.Id,
      OrderDate = o.OrderDate,
      TotalAmount = o.TotalAmount,
      Status = o.Status.ToString(),
      PaymentMethod = o.PaymentMethod,
      PaymentStatus = o.PaymentStatus,
      TrackingNumber = o.TrackingNumber,
      ShippingAddress = o.ShippingAddress,
      Items = o
        .OrderItems.Select(oi => new OrderItemDto
        {
          ProductId = oi.ProductId,
          ProductName = oi.Product?.Name ?? "Unknown",
          ProductImageUrl = oi.Product?.Images.FirstOrDefault()?.ImageUrl,
          Quantity = oi.Quantity,
          PriceAtPurchase = oi.PriceAtPurchase,
        })
        .ToList(),
    });
  }

  public async Task<OrderDto?> GetOrderByIdAsync(string userId, Guid id)
  {
    var o = await _orderRepository.GetByIdAndUserIdAsync(id, userId);
    if (o == null)
      return null;

    return new OrderDto
    {
      Id = o.Id,
      OrderDate = o.OrderDate,
      TotalAmount = o.TotalAmount,
      Status = o.Status.ToString(),
      PaymentMethod = o.PaymentMethod,
      PaymentStatus = o.PaymentStatus,
      TrackingNumber = o.TrackingNumber,
      ShippingAddress = o.ShippingAddress,
      Items = o
        .OrderItems.Select(oi => new OrderItemDto
        {
          ProductId = oi.ProductId,
          ProductName = oi.Product?.Name ?? "Unknown",
          ProductImageUrl = oi.Product?.Images.FirstOrDefault()?.ImageUrl,
          Quantity = oi.Quantity,
          PriceAtPurchase = oi.PriceAtPurchase,
        })
        .ToList(),
    };
  }
}
