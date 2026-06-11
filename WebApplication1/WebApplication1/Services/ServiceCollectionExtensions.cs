using WebApplication1.Repositories;
using WebApplication1.Mappers;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;

namespace WebApplication1.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrudServices(this IServiceCollection services)
    {
        services.AddScoped<ICrudMapper<User, UserRequest, UserResponse>, UserMapper>();
        services.AddScoped<ICrudMapper<Address, AddressRequest, AddressResponse>, AddressMapper>();
        services.AddScoped<ICrudMapper<AIChat, AIChatRequest, AIChatResponse>, AIChatMapper>();
        services.AddScoped<ICrudMapper<AIMessage, AIMessageRequest, AIMessageResponse>, AIMessageMapper>();
        services.AddScoped<ICrudMapper<Category, CategoryRequest, CategoryResponse>, CategoryMapper>();
        services.AddScoped<ICrudMapper<Product, ProductRequest, ProductResponse>, ProductMapper>();
        services.AddScoped<ICrudMapper<ProductImage, ProductImageRequest, ProductImageResponse>, ProductImageMapper>();
        services.AddScoped<ICrudMapper<ProductVariant, ProductVariantRequest, ProductVariantResponse>, ProductVariantMapper>();
        services.AddScoped<ICrudMapper<Cart, CartRequest, CartResponse>, CartMapper>();
        services.AddScoped<ICrudMapper<CartItem, CartItemRequest, CartItemResponse>, CartItemMapper>();
        services.AddScoped<ICrudMapper<Favorite, FavoriteRequest, FavoriteResponse>, FavoriteMapper>();
        services.AddScoped<ICrudMapper<Order, OrderRequest, OrderResponse>, OrderMapper>();
        services.AddScoped<ICrudMapper<OrderItem, OrderItemRequest, OrderItemResponse>, OrderItemMapper>();
        services.AddScoped<ICrudMapper<Payment, PaymentRequest, PaymentResponse>, PaymentMapper>();
        services.AddScoped<ICrudMapper<Review, ReviewRequest, ReviewResponse>, ReviewMapper>();
        services.AddScoped<ICrudMapper<RoomUpload, RoomUploadRequest, RoomUploadResponse>, RoomUploadMapper>();
        services.AddScoped<ICrudMapper<RoomDesign, RoomDesignRequest, RoomDesignResponse>, RoomDesignMapper>();
        services.AddScoped<ICrudMapper<RoomFurniturePlacement, RoomFurniturePlacementRequest, RoomFurniturePlacementResponse>, RoomFurniturePlacementMapper>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IAIChatRepository, AIChatRepository>();
        services.AddScoped<IAIChatService, AIChatService>();
        services.AddScoped<IAIMessageRepository, AIMessageRepository>();
        services.AddScoped<IAIMessageService, AIMessageService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IProductVariantService, ProductVariantService>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<ICartItemService, CartItemService>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderItemService, OrderItemService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IRoomUploadRepository, RoomUploadRepository>();
        services.AddScoped<IRoomUploadService, RoomUploadService>();
        services.AddScoped<IRoomDesignRepository, RoomDesignRepository>();
        services.AddScoped<IRoomDesignService, RoomDesignService>();
        services.AddScoped<IRoomFurniturePlacementRepository, RoomFurniturePlacementRepository>();
        services.AddScoped<IRoomFurniturePlacementService, RoomFurniturePlacementService>();

        return services;
    }
}
