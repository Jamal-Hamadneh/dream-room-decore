using WebApplication1.Contracts.Responses;

namespace WebApplication1.Services;

public interface IChatbotContextService
{
    Task<ChatbotContextResponse> GetContextAsync(int userId, CancellationToken cancellationToken = default);
    ChatwootConfigResponse GetChatwootConfig();
}
