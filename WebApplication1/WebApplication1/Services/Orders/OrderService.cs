using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IOrderService : ICrudService<OrderRequest, OrderResponse>
{
    Task<PagedResult<OrderResponse>> GetPagedAsync(int page, int limit, string? sort, CancellationToken cancellationToken = default);
}

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

    public async Task<PagedResult<OrderResponse>> GetPagedAsync(int page, int limit, string? sort, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        limit = limit < 1 ? 5 : limit;

        var query = ApplySort(Query(), sort);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(cancellationToken);

        return new PagedResult<OrderResponse>
        {
            Items = orders.Select(ToResponse).ToList(),
            Page = page,
            Limit = limit,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)limit)
        };
    }

    private static IQueryable<Order> ApplySort(IQueryable<Order> query, string? sort)
    {
        var (field, descending) = ParseSort(sort);
        return field switch
        {
            "totalprice" => descending ? query.OrderByDescending(order => order.TotalPrice) : query.OrderBy(order => order.TotalPrice),
            "status" => descending ? query.OrderByDescending(order => order.Status) : query.OrderBy(order => order.Status),
            _ => descending ? query.OrderByDescending(order => order.CreatedAt) : query.OrderBy(order => order.CreatedAt)
        };
    }

    private static (string Field, bool Descending) ParseSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return ("createdat", true);
        }

        var parts = sort.Split(':', 2);
        var field = parts[0].Trim().ToLowerInvariant();
        var descending = parts.Length < 2 || !string.Equals(parts[1].Trim(), "asc", StringComparison.OrdinalIgnoreCase);
        return (field, descending);
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
