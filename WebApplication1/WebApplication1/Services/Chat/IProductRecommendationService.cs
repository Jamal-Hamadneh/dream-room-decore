using WebApplication1.Models;

namespace WebApplication1.Services.Chat;

public interface IProductRecommendationService
{
    /// <summary>
    /// Finds active products that are likely relevant to the user's message, based on budget
    /// hints and furniture/category keywords. Falls back to top-rated featured products when
    /// nothing matches, so the assistant always has some catalog context to work with.
    /// </summary>
    Task<List<Product>> FindCandidatesAsync(string userMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes candidate products into a compact JSON representation suitable for the
    /// language model prompt.
    /// </summary>
    string BuildCatalogContext(IReadOnlyList<Product> products);
}
