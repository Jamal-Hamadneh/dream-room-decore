using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions;
using WebApplication1.Options;

namespace WebApplication1.Services;

public class ChatbotContextService(ApplicationDbContext context, IOptions<TawkOptions> tawkOptions) : IChatbotContextService
{
    public async Task<ChatbotContextResponse> GetContextAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException($"User '{userId}' was not found.");

        var cartItems = await context.CartItems
            .AsNoTracking()
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .Include(x => x.ProductVariant)
            .Where(x => x.Cart.UserId == userId)
            .Select(x => new ChatbotCartItemSummary
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                Price = x.ProductVariant != null ? x.ProductVariant.Price : x.Product.Price,
                VariantSku = x.ProductVariant != null ? x.ProductVariant.Sku : null
            })
            .ToListAsync(cancellationToken);

        var recentOrders = await context.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new ChatbotOrderSummary
            {
                OrderId = x.Id,
                TotalPrice = x.TotalPrice,
                Status = x.Status,
                PaymentStatus = x.PaymentStatus,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var roomDesigns = await context.RoomDesigns
            .AsNoTracking()
            .Include(x => x.RoomUpload)
            .Where(x => x.RoomUpload.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new ChatbotRoomDesignSummary
            {
                RoomDesignId = x.Id,
                RoomUploadId = x.RoomUploadId,
                Name = x.Name,
                RoomType = x.RoomUpload.RoomType,
                ImageUrl = x.RoomUpload.ImageUrl,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new ChatbotContextResponse
        {
            User = new ChatbotUserSummary
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Role = user.Role
            },
            CartItems = cartItems,
            RecentOrders = recentOrders,
            RoomDesigns = roomDesigns
        };
    }

    public TawkConfigResponse GetTawkConfig()
    {
        var propertyId = tawkOptions.Value.PropertyId;
        var widgetId = tawkOptions.Value.WidgetId;
        var isConfigured = !string.IsNullOrWhiteSpace(propertyId) && !string.IsNullOrWhiteSpace(widgetId);

        return new TawkConfigResponse
        {
            PropertyId = propertyId,
            WidgetId = widgetId,
            EmbedUrl = isConfigured ? $"https://embed.tawk.to/{propertyId}/{widgetId}" : string.Empty,
            IsConfigured = isConfigured
        };
    }
}
