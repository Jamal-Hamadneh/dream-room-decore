using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;

namespace WebApplication1.Services.Chat;

public interface IChatAssistantService
{
    Task<ChatMessageResponse> SendMessageAsync(int userId, SendChatMessageRequest request, CancellationToken cancellationToken = default);

    Task<List<ConversationSummaryResponse>> GetConversationsAsync(int userId, CancellationToken cancellationToken = default);

    Task<ConversationDetailResponse> GetConversationAsync(int userId, int conversationId, CancellationToken cancellationToken = default);

    Task<ConversationSummaryResponse> CreateConversationAsync(int userId, CreateConversationRequest request, CancellationToken cancellationToken = default);

    Task DeleteConversationAsync(int userId, int conversationId, CancellationToken cancellationToken = default);
}
