using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using WebApplication1.Options;

namespace WebApplication1.Services;

public class OpenAiService(HttpClient httpClient, IOptions<OpenAiOptions> options, IOptions<GeminiOptions> geminiOptions, IWebHostEnvironment environment, ILogger<OpenAiService> logger) : IOpenAiService
{
    private readonly OpenAiOptions _options = options.Value;
    private readonly GeminiOptions _geminiOptions = geminiOptions.Value;
    private readonly string _apiKey = FirstValue(options.Value.ApiKey, LoadLocalEnv(Path.Combine(environment.ContentRootPath, ".env")), "OPENAI_API_KEY");
    private readonly string _geminiApiKey = FirstValue(geminiOptions.Value.ApiKey, LoadLocalEnv(Path.Combine(environment.ContentRootPath, ".env")), "GEMINI_API_KEY");

    public async Task<OpenAiRoomResult> AnalyzeRoomDesignAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            try
            {
                var analysisJson = await AnalyzeRoomAsync(prompt, data, cancellationToken);
                return new OpenAiRoomResult { AnalysisJson = EnsureMode(analysisJson, "openai-analysis") };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "OpenAI room analysis failed. Trying Gemini provider next.");
            }
        }

        if (!string.IsNullOrWhiteSpace(_geminiApiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                var analysisJson = await AnalyzeRoomWithGeminiAsync(prompt, data, cancellationToken);
                return new OpenAiRoomResult { AnalysisJson = EnsureMode(analysisJson, "gemini-analysis") };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Gemini room analysis failed. Using mock fallback.");
            }
        }

        return CreateFallbackResult(data);
    }

    public async Task<byte[]?> GenerateRealisticRoomImageAsync(byte[] compositeImage, RoomAiPromptData data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return null;
        }

        try
        {
            using var form = new MultipartFormDataContent();

            var imageContent = new ByteArrayContent(compositeImage);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(imageContent, "image", "room.png");
            form.Add(new StringContent(_options.ImageModel), "model");
            form.Add(new StringContent("1536x1024"), "size");
            form.Add(new StringContent("high"), "quality");
            form.Add(new StringContent("high"), "input_fidelity");
            form.Add(new StringContent(BuildRenderPrompt(data)), "prompt");

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/edits")
            {
                Content = form
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("gpt-image-1 room render failed ({StatusCode}): {Body}", (int)response.StatusCode, body);
                return null;
            }

            using var document = JsonDocument.Parse(body);
            var base64 = document.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();
            return string.IsNullOrWhiteSpace(base64) ? null : Convert.FromBase64String(base64);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "gpt-image-1 room render failed.");
            return null;
        }
    }

    // Instructs gpt-image-1 to keep the real products and their placement untouched and only add
    // photorealistic lighting, shadows and perspective, so the result still shows the exact items
    // the customer placed (and that sit in their cart) rather than invented furniture.
    private static string BuildRenderPrompt(RoomAiPromptData data)
    {
        var productList = data.Products.Count == 0
            ? "the furniture shown"
            : string.Join(", ", data.Products.Select(product =>
            {
                var descriptors = new[] { product.Color, product.Material, product.Name }
                    .Where(value => !string.IsNullOrWhiteSpace(value));
                return string.Join(" ", descriptors);
            }));

        return
            "You are a professional interior-photography compositor. The provided image is a rough composite of a real room " +
            "with real furniture products placed into it. Re-render it as one cohesive, photorealistic interior photograph.\n\n" +
            "STRICT RULES:\n" +
            "- Keep every furniture item in the EXACT same position, size, scale and orientation as in the provided image.\n" +
            "- Preserve each product's exact shape, design, color, material and proportions. Do NOT redesign, recolor, swap, add or remove any furniture.\n" +
            "- Keep the room's walls, floor, windows and architecture unchanged.\n" +
            "- Only improve realism: integrate the furniture with correct perspective and grounding, consistent global lighting, " +
            "soft realistic contact shadows on the floor, and matching color temperature, so the items look genuinely photographed in the room instead of pasted.\n\n" +
            $"The furniture in the scene: {productList}.";
    }

    private async Task<string> AnalyzeRoomWithGeminiAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken)
    {
        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                                   text = "Return compact JSON only with roomType, roomLayout, wallColor, floorType, lighting, approximateWidth, approximateHeight, approximateDepth, exactProductsUsed, extraProductsAdded, colorRecommendations. " +
                                          "colorRecommendations must include palette, productColorAdvice, wallColorAdvice, and whyTheseColorsWork. " +
                                   "Use this room design image URL as visual reference: " + (data.PreviewImageUrl ?? data.RoomImageUrl) + "\n\n" +
                                   "Design prompt: " + prompt
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        using var response = await httpClient.PostAsJsonAsync(GeminiUrl(_geminiOptions.Model), request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini room analysis failed: {body}");
        }

        return ExtractGeminiText(body);
    }

    private async Task<string> AnalyzeRoomAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken)
    {
        var referenceImageUrl = data.PreviewImageUrl ?? data.RoomImageUrl;
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
                        new { type = "text", text = "Analyze this room image and return compact JSON with roomType, roomLayout, wallColor, floorType, lighting, approximateWidth, approximateHeight, approximateDepth, exactProductsUsed, extraProductsAdded, colorRecommendations. colorRecommendations must include palette, productColorAdvice, wallColorAdvice, and whyTheseColorsWork. Treat dimensions as approximate. Also consider this design prompt: " + prompt },
                        new { type = "image_url", image_url = new { url = referenceImageUrl } }
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
            AnalysisJson = JsonSerializer.Serialize(analysis)
        };
    }

    private string GeminiUrl(string model) => $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(_geminiApiKey)}";

    private static string ExtractGeminiText(string body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";
    }

    private static string EnsureMode(string json, string mode)
    {
        try
        {
            var node = JsonNode.Parse(json) as JsonObject;
            if (node is null)
            {
                return json;
            }

            node["mode"] = mode;
            return node.ToJsonString();
        }
        catch
        {
            return JsonSerializer.Serialize(new { analysis = json, mode });
        }
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
