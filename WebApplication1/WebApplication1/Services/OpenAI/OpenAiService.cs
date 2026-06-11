using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WebApplication1.Options;

namespace WebApplication1.Services;

public class OpenAiService(HttpClient httpClient, IOptions<OpenAiOptions> options, IWebHostEnvironment environment) : IOpenAiService
{
    private readonly OpenAiOptions _options = options.Value;
    private readonly string _apiKey = FirstValue(options.Value.ApiKey, LoadLocalEnv(Path.Combine(environment.ContentRootPath, ".env")), "OPENAI_API_KEY");

    public async Task<OpenAiRoomResult> GenerateRealisticRoomAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return CreateFallbackResult(data);
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var analysisJson = await AnalyzeRoomAsync(prompt, data, cancellationToken);
            var generatedImageDataUri = await GenerateImageAsync(prompt, data, cancellationToken);

            return new OpenAiRoomResult
            {
                AnalysisJson = analysisJson,
                GeneratedImageSourceUrl = data.RoomImageUrl,
                GeneratedImageDataUri = generatedImageDataUri
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return CreateFallbackResult(data);
        }
    }

    private async Task<string> AnalyzeRoomAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = _options.Model,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = "Analyze this room image and return compact JSON with roomType, roomLayout, wallColor, floorType, lighting, approximateWidth, approximateHeight, approximateDepth. Treat dimensions as approximate. Also consider this design prompt: " + prompt },
                        new { type = "image_url", image_url = new { url = data.RoomImageUrl } }
                    }
                }
            },
            response_format = new { type = "json_object" }
        };

        using var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI room analysis failed: {body}");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";
    }

    private async Task<string> GenerateImageAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken)
    {
        var imagePrompt = prompt + "\n\nCreate one realistic final room image. Use only the listed furniture. Keep the room structure, lighting, camera angle, and perspective as close as possible to the uploaded room image URL: " + data.RoomImageUrl;

        var request = new
        {
            model = _options.ImageModel,
            prompt = imagePrompt,
            size = "1024x1024"
        };

        using var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/images/generations", request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI image generation failed: {body}");
        }

        using var document = JsonDocument.Parse(body);
        var base64 = document.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();

        if (string.IsNullOrWhiteSpace(base64))
        {
            throw new InvalidOperationException("OpenAI image generation did not return image data.");
        }

        return $"data:image/png;base64,{base64}";
    }

    private static OpenAiRoomResult CreateFallbackResult(RoomAiPromptData data)
    {
        var analysis = new
        {
            roomType = string.IsNullOrWhiteSpace(data.RoomType) ? "unknown" : data.RoomType,
            roomLayout = "Estimated from uploaded image and user placements.",
            wallColor = "approximate",
            floorType = "approximate",
            lighting = "approximate",
            approximateWidth = data.Width,
            approximateHeight = data.Height,
            approximateDepth = data.Depth,
            mode = "mock"
        };

        return new OpenAiRoomResult
        {
            AnalysisJson = JsonSerializer.Serialize(analysis),
            GeneratedImageSourceUrl = data.RoomImageUrl
        };
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
