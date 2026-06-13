using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IOrderService : ICrudService<OrderRequest, OrderResponse>;

public class OrderService(IOrderRepository repository, ICrudMapper<Order, OrderRequest, OrderResponse> mapper, ApplicationDbContext context)
    : CrudService<Order, OrderRequest, OrderResponse>(repository, mapper), IOrderService
{
    public override async Task<List<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await Query().ToListAsync(cancellationToken);
        return orders.Select(ToResponse).ToList();
    }

    public override async Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await Query().FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
        return order is null ? null : ToResponse(order);
    }

    public override async Task<OrderResponse> CreateAsync(OrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            UserId = request.UserId,
            AddressId = request.AddressId,
            TotalPrice = request.TotalPrice,
            Status = request.Status,
            PaymentStatus = request.PaymentStatus,
            StripePaymentIntentId = request.StripePaymentIntentId,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(order.Id, cancellationToken))!;
    }

    public override async Task<OrderResponse?> UpdateAsync(int id, OrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders.FindAsync([id], cancellationToken);
        if (order is null)
        {
            return null;
        }

        order.UserId = request.UserId;
        order.AddressId = request.AddressId;
        order.TotalPrice = request.TotalPrice;
        order.Status = request.Status;
        order.PaymentStatus = request.PaymentStatus;
        order.StripePaymentIntentId = request.StripePaymentIntentId;
        order.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private IQueryable<Order> Query() => context.Orders
        .AsNoTracking()
        .Include(order => order.User)
        .Include(order => order.Address)
        .Include(order => order.Payment)
        .Include(order => order.OrderItems)
            .ThenInclude(item => item.Product)
                .ThenInclude(product => product.ProductImages)
        .Include(order => order.OrderItems)
            .ThenInclude(item => item.ProductVariant);

    private static OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        AddressId = order.AddressId,
        TotalPrice = order.TotalPrice,
        Status = order.Status,
        PaymentStatus = order.PaymentStatus,
        StripePaymentIntentId = order.StripePaymentIntentId,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
        User = ResponseMapping.ToUserSummary(order.User),
        Address = ResponseMapping.ToAddressSummary(order.Address),
        Items = order.OrderItems.Select(ResponseMapping.ToOrderItemSummary).ToList(),
        Payment = order.Payment is null ? null : ResponseMapping.ToPaymentSummary(order.Payment)
    };
}
