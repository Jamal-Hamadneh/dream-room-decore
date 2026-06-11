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
