using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WebApplication1.Models;

namespace WebApplication1.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AIChat> AIChats => Set<AIChat>();
    public DbSet<AIMessage> AIMessages => Set<AIMessage>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RoomUpload> RoomUploads => Set<RoomUpload>();
    public DbSet<RoomDesign> RoomDesigns => Set<RoomDesign>();
    public DbSet<RoomFurniturePlacement> RoomFurniturePlacements => Set<RoomFurniturePlacement>();
    public DbSet<GeneratedRoomImage> GeneratedRoomImages => Set<GeneratedRoomImage>();
    public DbSet<RoomDesignReplacement> RoomDesignReplacements => Set<RoomDesignReplacement>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RevokedAccessToken> RevokedAccessTokens => Set<RevokedAccessToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureProducts(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureRooms(modelBuilder);
        ConfigureSchema(modelBuilder);
    }

    private static void ConfigureSchema(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
            {
                var storeObject = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
                property.SetColumnName(ToSnakeCase(property.GetColumnName(storeObject)!));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()!));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()!));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }

        modelBuilder.Entity<Cart>().ToTable("cart");
        modelBuilder.Entity<AIMessage>().Property(message => message.AIChatId).HasColumnName("chat_id");
        modelBuilder.Entity<CartItem>().Property(item => item.ProductVariantId).HasColumnName("variant_id");
        modelBuilder.Entity<OrderItem>().Property(item => item.ProductVariantId).HasColumnName("variant_id");
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .ToTable(table => table.HasCheckConstraint("ck_users_role", "role IN ('customer', 'admin')"));

        modelBuilder.Entity<User>()
            .HasOne(user => user.Cart)
            .WithOne(cart => cart.User)
            .HasForeignKey<Cart>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(user => user.AIChats)
            .WithOne(chat => chat.User)
            .HasForeignKey(chat => chat.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AIChat>()
            .HasMany(chat => chat.AIMessages)
            .WithOne(message => message.AIChat)
            .HasForeignKey(message => message.AIChatId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Addresses)
            .WithOne(address => address.User)
            .HasForeignKey(address => address.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Address>()
            .HasMany(address => address.Orders)
            .WithOne(order => order.Address)
            .HasForeignKey(order => order.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Favorites)
            .WithOne(favorite => favorite.User)
            .HasForeignKey(favorite => favorite.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Orders)
            .WithOne(order => order.User)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(user => user.Reviews)
            .WithOne(review => review.User)
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(user => user.RoomUploads)
            .WithOne(upload => upload.User)
            .HasForeignKey(upload => upload.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasMany(user => user.RefreshTokens)
            .WithOne(refreshToken => refreshToken.User)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        modelBuilder.Entity<RevokedAccessToken>()
            .HasIndex(token => token.JwtId)
            .IsUnique();
    }

    private static void ConfigureProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.Property(product => product.DiscountPrice).HasPrecision(18, 2);
            entity.Property(product => product.Height).HasPrecision(18, 2);
            entity.Property(product => product.Width).HasPrecision(18, 2);
            entity.Property(product => product.Depth).HasPrecision(18, 2);
            entity.Property(product => product.AverageRating).HasPrecision(3, 2);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(variant => variant.Sku).IsUnique();
            entity.Property(variant => variant.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Review>()
            .ToTable(table => table.HasCheckConstraint("ck_reviews_rating", "rating BETWEEN 1 AND 5"));

        modelBuilder.Entity<Category>()
            .HasMany(category => category.Products)
            .WithOne(product => product.Category)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(product => product.ProductImages)
            .WithOne(image => image.Product)
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(product => product.ProductVariants)
            .WithOne(variant => variant.Product)
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(product => product.CartItems)
            .WithOne(item => item.Product)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductVariant>()
            .HasMany(variant => variant.CartItems)
            .WithOne(item => item.ProductVariant)
            .HasForeignKey(item => item.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(product => product.Favorites)
            .WithOne(favorite => favorite.Product)
            .HasForeignKey(favorite => favorite.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(product => product.OrderItems)
            .WithOne(item => item.Product)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductVariant>()
            .HasMany(variant => variant.OrderItems)
            .WithOne(item => item.ProductVariant)
            .HasForeignKey(item => item.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(product => product.Reviews)
            .WithOne(review => review.Product)
            .HasForeignKey(review => review.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(product => product.RoomFurniturePlacements)
            .WithOne(placement => placement.Product)
            .HasForeignKey(placement => placement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>()
            .HasIndex(cart => cart.UserId)
            .IsUnique();

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.TotalPrice).HasPrecision(18, 2);
            entity.ToTable(table => table.HasCheckConstraint("ck_orders_status", "status IN ('pending', 'processing', 'shipped', 'delivered', 'cancelled')"));
            entity.ToTable(table => table.HasCheckConstraint("ck_orders_payment_status", "payment_status IN ('pending', 'paid', 'failed')"));
        });

        modelBuilder.Entity<OrderItem>()
            .Property(item => item.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(payment => payment.OrderId).IsUnique();
            entity.Property(payment => payment.Amount).HasPrecision(18, 2);
            entity.ToTable(table => table.HasCheckConstraint("ck_payments_status", "status IN ('pending', 'succeeded', 'failed')"));
        });

        modelBuilder.Entity<Cart>()
            .HasMany(cart => cart.CartItems)
            .WithOne(item => item.Cart)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasMany(order => order.OrderItems)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(order => order.Payment)
            .WithOne(payment => payment.Order)
            .HasForeignKey<Payment>(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureRooms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoomUpload>(entity =>
        {
            entity.Property(upload => upload.Height).HasPrecision(18, 2);
            entity.Property(upload => upload.Width).HasPrecision(18, 2);
            entity.Property(upload => upload.Depth).HasPrecision(18, 2);
            entity.Property(upload => upload.AiDetectedWidth).HasPrecision(18, 2);
            entity.Property(upload => upload.AiDetectedHeight).HasPrecision(18, 2);
            entity.Property(upload => upload.AiDetectedDepth).HasPrecision(18, 2);
        });

        modelBuilder.Entity<RoomFurniturePlacement>(entity =>
        {
            entity.Property(placement => placement.PositionX).HasPrecision(18, 2);
            entity.Property(placement => placement.PositionY).HasPrecision(18, 2);
            entity.Property(placement => placement.Rotation).HasPrecision(18, 2);
            entity.Property(placement => placement.Scale).HasPrecision(18, 2);
        });

        modelBuilder.Entity<GeneratedRoomImage>()
            .ToTable(table => table.HasCheckConstraint("ck_generated_room_images_status", "status IN ('pending', 'completed', 'failed')"));

        modelBuilder.Entity<RoomDesignReplacement>()
            .ToTable(table => table.HasCheckConstraint("ck_room_design_replacements_status", "status IN ('pending', 'completed', 'failed')"));

        modelBuilder.Entity<RoomUpload>()
            .HasMany(upload => upload.RoomDesigns)
            .WithOne(design => design.RoomUpload)
            .HasForeignKey(design => design.RoomUploadId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomDesign>()
            .HasMany(design => design.RoomFurniturePlacements)
            .WithOne(placement => placement.RoomDesign)
            .HasForeignKey(placement => placement.RoomDesignId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomDesign>()
            .HasMany(design => design.GeneratedRoomImages)
            .WithOne(image => image.RoomDesign)
            .HasForeignKey(image => image.RoomDesignId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomDesign>()
            .HasMany(design => design.RoomDesignReplacements)
            .WithOne(replacement => replacement.RoomDesign)
            .HasForeignKey(replacement => replacement.RoomDesignId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomDesignReplacement>()
            .HasOne(replacement => replacement.OldProduct)
            .WithMany()
            .HasForeignKey(replacement => replacement.OldProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomDesignReplacement>()
            .HasOne(replacement => replacement.NewProduct)
            .WithMany()
            .HasForeignKey(replacement => replacement.NewProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var result = new List<char>(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (char.IsUpper(current))
            {
                var hasPrevious = i > 0;
                var hasNext = i + 1 < value.Length;
                var previousIsLowerOrDigit = hasPrevious && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1]));
                var nextIsLower = hasNext && char.IsLower(value[i + 1]);

                if (hasPrevious && (previousIsLowerOrDigit || nextIsLower))
                {
                    result.Add('_');
                }

                result.Add(char.ToLowerInvariant(current));
            }
            else
            {
                result.Add(current);
            }
        }

        return new string(result.ToArray());
    }
}
