namespace WebApplication1.Services.OpenAI;

public interface IChatCompletionService
{
    /// <summary>
    /// Requests a structured assistant reply from the language model. Returns null if no API
    /// key is configured or the request fails after retries, so callers can fall back gracefully.
    /// </summary>
    Task<ChatAssistantReply?> GetReplyAsync(
        string systemPrompt,
        IReadOnlyList<(string Role, string Content)> history,
        string catalogContextJson,
        string userMessage,
        CancellationToken cancellationToken = default);
}
