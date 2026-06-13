using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services.Chat;

public class ProductRecommendationService(ApplicationDbContext context) : IProductRecommendationService
{
    private const int MaxCandidates = 8;

    private static readonly string[] FurnitureKeywords =
    [
        "sofa", "couch", "sectional", "loveseat", "armchair", "recliner", "chair",
        "table", "dining table", "coffee table", "side table", "desk", "console",
        "bed", "mattress", "headboard", "nightstand", "dresser", "wardrobe", "closet",
        "bookshelf", "bookcase", "shelf", "shelving", "cabinet", "sideboard", "credenza",
        "lamp", "lighting", "chandelier", "pendant", "ottoman", "bench", "stool",
        "mirror", "rug", "carpet", "curtain"
    ];

    private static readonly Regex BetweenPattern = new(@"between\s+\$?(\d+(?:\.\d+)?)\s*(?:and|-|to)\s*\$?(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RangePattern = new(@"\$(\d+(?:\.\d+)?)\s*-\s*\$?(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MaxPattern = new(@"(?:under|below|less than|max(?:imum)?|up to|budget(?: of)?)\s*\$?(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MinPattern = new(@"(?:over|above|at least|more than)\s*\$?(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<List<Product>> FindCandidatesAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var products = await context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Where(product => product.IsActive)
            .ToListAsync(cancellationToken);

        var (minPrice, maxPrice) = ExtractBudget(userMessage);
        var keywords = await ExtractKeywordsAsync(userMessage, cancellationToken);

        IEnumerable<Product> filtered = products;

        if (keywords.Count > 0)
        {
            filtered = filtered.Where(product =>
                keywords.Any(keyword =>
                    product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    product.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    product.Category.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        if (minPrice.HasValue)
        {
            filtered = filtered.Where(product => (product.DiscountPrice ?? product.Price) >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            filtered = filtered.Where(product => (product.DiscountPrice ?? product.Price) <= maxPrice.Value);
        }

        var results = OrderByRelevance(filtered).Take(MaxCandidates).ToList();

        return results.Count > 0 ? results : OrderByRelevance(products).Take(MaxCandidates).ToList();
    }

    public string BuildCatalogContext(IReadOnlyList<Product> products)
    {
        var items = products.Select(product => new CatalogContextItem(
            product.Id,
            product.Name,
            product.Category.Name,
            product.Price,
            product.DiscountPrice,
            product.Material,
            product.Color,
            FormatDimensions(product.Width, product.Height, product.Depth),
            product.StockQuantity,
            product.Description));

        return JsonSerializer.Serialize(items);
    }

    private static IOrderedEnumerable<Product> OrderByRelevance(IEnumerable<Product> products) => products
        .OrderByDescending(product => product.IsFeatured)
        .ThenByDescending(product => product.AverageRating)
        .ThenBy(product => product.DiscountPrice ?? product.Price);

    private async Task<List<string>> ExtractKeywordsAsync(string message, CancellationToken cancellationToken)
    {
        var lowerMessage = message.ToLowerInvariant();
        var matches = new List<string>();

        foreach (var term in FurnitureKeywords)
        {
            if (lowerMessage.Contains(term))
            {
                matches.Add(term);
            }
        }

        var categoryNames = await context.Categories.AsNoTracking().Select(category => category.Name).ToListAsync(cancellationToken);
        foreach (var name in categoryNames)
        {
            if (!string.IsNullOrWhiteSpace(name) && lowerMessage.Contains(name.ToLowerInvariant()))
            {
                matches.Add(name);
            }
        }

        return matches.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static (decimal? Min, decimal? Max) ExtractBudget(string message)
    {
        var betweenMatch = BetweenPattern.Match(message);
        if (betweenMatch.Success)
        {
            return ToOrderedRange(betweenMatch);
        }

        var rangeMatch = RangePattern.Match(message);
        if (rangeMatch.Success)
        {
            return ToOrderedRange(rangeMatch);
        }

        decimal? min = null;
        decimal? max = null;

        var maxMatch = MaxPattern.Match(message);
        if (maxMatch.Success)
        {
            max = decimal.Parse(maxMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        var minMatch = MinPattern.Match(message);
        if (minMatch.Success)
        {
            min = decimal.Parse(minMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        return (min, max);
    }

    private static (decimal? Min, decimal? Max) ToOrderedRange(Match match)
    {
        var a = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var b = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        return (Math.Min(a, b), Math.Max(a, b));
    }

    private static string? FormatDimensions(decimal? width, decimal? height, decimal? depth)
    {
        if (width is null && height is null && depth is null)
        {
            return null;
        }

        static string Part(decimal? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "?";

        return $"W:{Part(width)} x H:{Part(height)} x D:{Part(depth)}";
    }

    private record CatalogContextItem(
        int Id,
        string Name,
        string Category,
        decimal Price,
        decimal? DiscountPrice,
        string? Material,
        string? Color,
        string? Dimensions,
        int StockQuantity,
        string Description);
}
