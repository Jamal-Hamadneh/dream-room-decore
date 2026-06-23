using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using WebApplication1.Options;

namespace WebApplication1.Services.OpenAI;

public class ChatCompletionService : IChatCompletionService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan[] RetryDelays = [TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1500)];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly BinaryData ReplySchema = BinaryData.FromString("""
        {
          "type": "object",
          "properties": {
            "message": { "type": "string" },
            "recommendedProductIds": {
              "type": "array",
              "items": { "type": "integer" }
            }
          },
          "required": ["message", "recommendedProductIds"],
          "additionalProperties": false
        }
        """);

    private readonly ChatClient? _chatClient;
    private readonly ILogger<ChatCompletionService> _logger;

    public ChatCompletionService(IOptions<OpenAiOptions> options, IWebHostEnvironment environment, ILogger<ChatCompletionService> logger)
    {
        _logger = logger;

        var apiKey = FirstValue(options.Value.ApiKey, LoadLocalEnv(Path.Combine(environment.ContentRootPath, ".env")), "OPENAI_API_KEY");

        _chatClient = string.IsNullOrWhiteSpace(apiKey)
            ? null
            : new ChatClient(options.Value.ChatModel, apiKey);
    }

    public async Task<ChatAssistantReply?> GetReplyAsync(
        string systemPrompt,
        IReadOnlyList<(string Role, string Content)> history,
        string catalogContextJson,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (_chatClient is null)
        {
            return null;
        }

        var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };

        foreach (var (role, content) in history)
        {
            messages.Add(role == "assistant" ? new AssistantChatMessage(content) : new UserChatMessage(content));
        }

        messages.Add(new UserChatMessage($"[CATALOG CONTEXT]\n{catalogContextJson}\n\n[USER MESSAGE]\n{userMessage}"));

        var completionOptions = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "furniture_assistant_reply",
                ReplySchema,
                jsonSchemaIsStrict: true)
        };

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                var completion = await _chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken);
                var json = completion.Value.Content[0].Text;
                return JsonSerializer.Deserialize<ChatAssistantReply>(json, JsonOptions);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(exception, "OpenAI chat completion attempt {Attempt} failed. Retrying.", attempt);
                await Task.Delay(RetryDelays[attempt - 1], cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "OpenAI chat completion failed after {Attempts} attempts.", MaxAttempts);
            }
        }

        return null;
    }

    private static Dictionary<string, string> LoadLocalEnv(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, string>();
        }

        return File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith('#'))
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }

    private static string FirstValue(string configuredValue, Dictionary<string, string> localEnv, string key)
    {
        if (!string.IsNullOrWhiteSpace(configuredValue))
        {
            return configuredValue;
        }

        return localEnv.TryGetValue(key, out var value) ? value : string.Empty;
    }
}
