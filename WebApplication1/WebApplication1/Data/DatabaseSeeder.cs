using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<User>>();

        await context.Database.MigrateAsync();

        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var customer = new User
        {
            FirstName = "Karam",
            LastName = "Customer",
            Email = "customer@dreamroom.test",
            Phone = "+962790000001",
            Role = "customer",
            CreatedAt = now
        };
        customer.PasswordHash = passwordHasher.HashPassword(customer, "Password123");

        var admin = new User
        {
            FirstName = "Dream",
            LastName = "Admin",
            Email = "admin@dreamroom.test",
            Phone = "+962790000002",
            Role = "admin",
            CreatedAt = now
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin12345");

        context.Users.AddRange(customer, admin);
        await context.SaveChangesAsync();

        var address = new Address
        {
            UserId = customer.Id,
            Country = "Jordan",
            City = "Amman",
            Street = "Queen Rania Street",
            Building = "Dream Building 12",
            PostalCode = "11941",
            IsDefault = true,
            CreatedAt = now
        };

        context.Addresses.Add(address);

        var livingRoom = new Category { Name = "Living Room", CreatedAt = now };
        var bedroom = new Category { Name = "Bedroom", CreatedAt = now };
        var lighting = new Category { Name = "Lighting", CreatedAt = now };
        var office = new Category { Name = "Office", CreatedAt = now };
        var storage = new Category { Name = "Storage", CreatedAt = now };

        context.Categories.AddRange(livingRoom, bedroom, lighting, office, storage);
        await context.SaveChangesAsync();

        var sofa = new Product
        {
            CategoryId = livingRoom.Id,
            Name = "Modern Beige Sofa",
            Description = "Three-seat sofa with soft linen upholstery for modern living rooms.",
            Price = 650m,
            DiscountPrice = 590m,
            StockQuantity = 12,
            Material = "Linen",
            Color = "Beige",
            Height = 85m,
            Width = 220m,
            Depth = 95m,
            IsActive = true,
            IsFeatured = true,
            AverageRating = 4.7m,
            ReviewsCount = 1,
            CreatedAt = now
        };

        var table = new Product
        {
            CategoryId = livingRoom.Id,
            Name = "Walnut Coffee Table",
            Description = "Low-profile walnut coffee table with rounded edges.",
            Price = 180m,
            StockQuantity = 20,
            Material = "Wood",
            Color = "Walnut",
            Height = 42m,
            Width = 110m,
            Depth = 60m,
            IsActive = true,
            IsFeatured = true,
            AverageRating = 4.5m,
            ReviewsCount = 0,
            CreatedAt = now
        };

        var bed = new Product
        {
            CategoryId = bedroom.Id,
            Name = "Queen Platform Bed",
            Description = "Minimal queen platform bed with upholstered headboard.",
            Price = 480m,
            StockQuantity = 8,
            Material = "Fabric and wood",
            Color = "Gray",
            Height = 105m,
            Width = 165m,
            Depth = 210m,
            IsActive = true,
            IsFeatured = false,
            AverageRating = 4.3m,
            ReviewsCount = 0,
            CreatedAt = now
        };

        var lamp = new Product
        {
            CategoryId = lighting.Id,
            Name = "Arc Floor Lamp",
            Description = "Black metal arc floor lamp for cozy reading corners.",
            Price = 95m,
            StockQuantity = 25,
            Material = "Metal",
            Color = "Black",
            Height = 180m,
            Width = 35m,
            Depth = 35m,
            IsActive = true,
            IsFeatured = true,
            AverageRating = 4.8m,
            ReviewsCount = 0,
            CreatedAt = now
        };

        var loveseat = new Product
        {
            CategoryId = livingRoom.Id,
            Name = "Compact Linen Loveseat",
            Description = "Two-seat loveseat with slim arms, perfect for small apartments and studios.",
            Price = 460m,
            StockQuantity = 15,
            Material = "Linen",
            Color = "Light Gray",
            Height = 80m,
            Width = 145m,
            Depth = 85m,
            IsActive = true,
            IsFeatured = true,
            AverageRating = 4.6m,
            ReviewsCount = 0,
            CreatedAt = now
        };

        var desk = new Product
        {
            CategoryId = office.Id,
            Name = "Scandinavian Study Desk",
            Description = "Compact study desk with a single drawer, ideal for home offices and small rooms.",
            Price = 210m,
            StockQuantity = 18,
            Material = "Wood",
            Color = "Oak",
            Height = 75m,
            Width = 110m,
            Depth = 55m,
            IsActive = true,
            IsFeatured = false,
            AverageRating = 4.4m,
            ReviewsCount = 0,
            CreatedAt = now
        };

        var bookshelf = new Product
        {
            CategoryId = storage.Id,
            Name = "Oak Ladder Bookshelf",
            Description = "Five-tier leaning bookshelf with a slim footprint, great for living rooms and home offices.",
            Price = 240m,
            StockQuantity = 14,
            Material = "Wood",
            Color = "Oak",
            Height = 180m,
            Width = 70m,
            Depth = 30m,
            IsActive = true,
            IsFeatured = true,
            AverageRating = 4.5m,
            ReviewsCount = 0,
            CreatedAt = now
        };

        context.Products.AddRange(sofa, table, bed, lamp, loveseat, desk, bookshelf);
        await context.SaveChangesAsync();

        context.ProductImages.AddRange(
            new ProductImage { ProductId = sofa.Id, ImageUrl = "https://example.com/images/modern-beige-sofa-main.jpg", IsMain = true, CreatedAt = now },
            new ProductImage { ProductId = table.Id, ImageUrl = "https://example.com/images/walnut-coffee-table-main.jpg", IsMain = true, CreatedAt = now },
            new ProductImage { ProductId = bed.Id, ImageUrl = "https://example.com/images/queen-platform-bed-main.jpg", IsMain = true, CreatedAt = now },
            new ProductImage { ProductId = lamp.Id, ImageUrl = "https://example.com/images/arc-floor-lamp-main.jpg", IsMain = true, CreatedAt = now },
            new ProductImage { ProductId = loveseat.Id, ImageUrl = "https://example.com/images/compact-linen-loveseat-main.jpg", IsMain = true, CreatedAt = now },
            new ProductImage { ProductId = desk.Id, ImageUrl = "https://example.com/images/scandinavian-study-desk-main.jpg", IsMain = true, CreatedAt = now },
            new ProductImage { ProductId = bookshelf.Id, ImageUrl = "https://example.com/images/oak-ladder-bookshelf-main.jpg", IsMain = true, CreatedAt = now });

        var beigeSofaVariant = new ProductVariant
        {
            ProductId = sofa.Id,
            Color = "Beige",
            Size = "3 seats",
            Material = "Linen",
            Sku = "SOFA-BEIGE-3S",
            Price = 590m,
            StockQuantity = 6,
            CreatedAt = now
        };

        var graySofaVariant = new ProductVariant
        {
            ProductId = sofa.Id,
            Color = "Gray",
            Size = "3 seats",
            Material = "Linen",
            Sku = "SOFA-GRAY-3S",
            Price = 620m,
            StockQuantity = 6,
            CreatedAt = now
        };

        var blackLampVariant = new ProductVariant
        {
            ProductId = lamp.Id,
            Color = "Black",
            Size = "Standard",
            Material = "Metal",
            Sku = "LAMP-BLACK-STD",
            Price = 95m,
            StockQuantity = 15,
            CreatedAt = now
        };

        context.ProductVariants.AddRange(beigeSofaVariant, graySofaVariant, blackLampVariant);
        await context.SaveChangesAsync();

        var cart = new Cart
        {
            UserId = customer.Id,
            CreatedAt = now
        };

        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        context.CartItems.AddRange(
            new CartItem { CartId = cart.Id, ProductId = sofa.Id, ProductVariantId = beigeSofaVariant.Id, Quantity = 1, CreatedAt = now },
            new CartItem { CartId = cart.Id, ProductId = table.Id, Quantity = 1, CreatedAt = now },
            new CartItem { CartId = cart.Id, ProductId = lamp.Id, ProductVariantId = blackLampVariant.Id, Quantity = 2, CreatedAt = now });

        context.Favorites.AddRange(
            new Favorite { UserId = customer.Id, ProductId = sofa.Id, CreatedAt = now },
            new Favorite { UserId = customer.Id, ProductId = lamp.Id, CreatedAt = now });

        context.Reviews.Add(new Review
        {
            UserId = customer.Id,
            ProductId = sofa.Id,
            Rating = 5,
            Comment = "Comfortable and fits my living room perfectly.",
            CreatedAt = now
        });

        await context.SaveChangesAsync();

        var order = new Order
        {
            UserId = customer.Id,
            AddressId = address.Id,
            TotalPrice = 685m,
            Status = "processing",
            PaymentStatus = "paid",
            StripePaymentIntentId = "pi_seed_001",
            CreatedAt = now
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        context.OrderItems.AddRange(
            new OrderItem { OrderId = order.Id, ProductId = sofa.Id, ProductVariantId = beigeSofaVariant.Id, Quantity = 1, Price = 590m, CreatedAt = now },
            new OrderItem { OrderId = order.Id, ProductId = lamp.Id, ProductVariantId = blackLampVariant.Id, Quantity = 1, Price = 95m, CreatedAt = now });

        context.Payments.Add(new Payment
        {
            OrderId = order.Id,
            StripePaymentIntentId = "pi_seed_001",
            StripeChargeId = "ch_seed_001",
            Amount = 685m,
            Currency = "JOD",
            Status = "succeeded",
            CreatedAt = now
        });

        await context.SaveChangesAsync();
    }
}
