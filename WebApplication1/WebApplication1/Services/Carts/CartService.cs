using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions.Carts;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface ICartService : ICrudService<CartRequest, CartResponse>;

public class CartService(ICartRepository repository, ICrudMapper<Cart, CartRequest, CartResponse> mapper, ApplicationDbContext context)
    : CrudService<Cart, CartRequest, CartResponse>(repository, mapper), ICartService
{
    public override async Task<List<CartResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var carts = await Query().ToListAsync(cancellationToken);
        return carts.Select(ToResponse).ToList();
    }

    public override async Task<CartResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cart = await Query().FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);
        return cart is null ? null : ToResponse(cart);
    }

    public override async Task<CartResponse> CreateAsync(CartRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await context.Carts.AnyAsync(cart => cart.UserId == request.UserId, cancellationToken);
        if (exists)
        {
            throw new UserCartAlreadyExistsException(request.UserId);
        }

        return await base.CreateAsync(request, cancellationToken);
    }

    private IQueryable<Cart> Query() => context.Carts
        .AsNoTracking()
        .Include(cart => cart.User)
        .Include(cart => cart.CartItems)
            .ThenInclude(item => item.Product)
                .ThenInclude(product => product.ProductImages)
        .Include(cart => cart.CartItems)
            .ThenInclude(item => item.ProductVariant);

    private static CartResponse ToResponse(Cart cart)
    {
        var items = cart.CartItems.Select(ResponseMapping.ToCartItemSummary).ToList();

        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            User = ResponseMapping.ToUserSummary(cart.User),
            Items = items,
            ItemsCount = items.Sum(item => item.Quantity),
            TotalPrice = items.Sum(item => item.TotalPrice)
        };
    }
}
