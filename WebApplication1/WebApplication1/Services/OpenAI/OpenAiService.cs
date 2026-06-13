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

    public async Task<OpenAiRoomResult> GenerateRealisticRoomFromPreviewAsync(string prompt, RoomAiPromptData data, Stream previewImageStream, string previewImageContentType, string previewImageFileName, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            try
            {
                var analysisJson = await AnalyzeRoomAsync(prompt, data, cancellationToken);
                var generatedImageDataUri = await GenerateImageEditAsync(prompt, previewImageStream, previewImageContentType, previewImageFileName, cancellationToken);

                return new OpenAiRoomResult
                {
                    AnalysisJson = EnsureMode(analysisJson, "openai-preview"),
                    GeneratedImageSourceUrl = data.PreviewImageUrl ?? data.RoomImageUrl,
                    GeneratedImageDataUri = generatedImageDataUri
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "OpenAI preview image generation failed. Trying Gemini provider next.");
            }
        }

        if (!string.IsNullOrWhiteSpace(_geminiApiKey))
        {
            try
            {
                return await GenerateRealisticRoomWithGeminiAsync(prompt, data, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Gemini room generation failed. Using preview/mock fallback.");
            }
        }

        return CreateFallbackResult(data);
    }

    private async Task<OpenAiRoomResult> GenerateRealisticRoomWithGeminiAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = null;

        var analysisJson = await AnalyzeRoomWithGeminiAsync(prompt, data, cancellationToken);
        var generatedImageDataUri = await GenerateImageWithGeminiAsync(prompt, data, cancellationToken);

        return new OpenAiRoomResult
        {
            AnalysisJson = EnsureMode(analysisJson, "gemini"),
            GeneratedImageSourceUrl = data.RoomImageUrl,
            GeneratedImageDataUri = generatedImageDataUri
        };
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
                                   "Use this uploaded room image URL as visual reference: " + data.RoomImageUrl + "\n\n" +
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

    private async Task<string> GenerateImageWithGeminiAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken)
    {
        var imagePrompt = prompt + "\n\nCreate one realistic final room image. Use only the listed furniture. " +
                          "Keep the room structure, lighting, camera angle, and perspective close to this uploaded room image URL: " + data.RoomImageUrl + "\n\n" +
                          "Furniture to include: " + string.Join("; ", data.Products.Select(product => $"{product.Name}, {product.Color}, {product.Material}, image: {product.ImageUrl}"));

        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = imagePrompt }
                    }
                }
            },
            generationConfig = new
            {
                responseModalities = new[] { "TEXT", "IMAGE" }
            }
        };

        using var response = await httpClient.PostAsJsonAsync(GeminiUrl(_geminiOptions.ImageModel), request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini image generation failed: {body}");
        }

        using var document = JsonDocument.Parse(body);
        foreach (var candidate in document.RootElement.GetProperty("candidates").EnumerateArray())
        {
            foreach (var part in candidate.GetProperty("content").GetProperty("parts").EnumerateArray())
            {
                if (!part.TryGetProperty("inlineData", out var inlineData))
                {
                    continue;
                }

                var mimeType = inlineData.GetProperty("mimeType").GetString() ?? "image/png";
                var base64 = inlineData.GetProperty("data").GetString();
                if (!string.IsNullOrWhiteSpace(base64))
                {
                    return $"data:{mimeType};base64,{base64}";
                }
            }
        }

        throw new InvalidOperationException("Gemini image generation did not return image data.");
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

    private async Task<string> GenerateImageEditAsync(string prompt, Stream previewImageStream, string previewImageContentType, string previewImageFileName, CancellationToken cancellationToken)
    {
        using var request = new MultipartFormDataContent
        {
            { new StringContent(_options.ImageModel), "model" },
            { new StringContent(prompt), "prompt" },
            { new StringContent("1024x1024"), "size" },
            { new StringContent("high"), "input_fidelity" }
        };

        var imageContent = new StreamContent(previewImageStream);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(string.IsNullOrWhiteSpace(previewImageContentType) ? "image/png" : previewImageContentType);
        request.Add(imageContent, "image", string.IsNullOrWhiteSpace(previewImageFileName) ? "preview.png" : previewImageFileName);

        using var response = await httpClient.PostAsync("https://api.openai.com/v1/images/edits", request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI preview image generation failed: {body}");
        }

        using var document = JsonDocument.Parse(body);
        var base64 = document.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();

        if (string.IsNullOrWhiteSpace(base64))
        {
            throw new InvalidOperationException("OpenAI preview image generation did not return image data.");
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
            GeneratedImageSourceUrl = data.PreviewImageUrl ?? data.RoomImageUrl
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
