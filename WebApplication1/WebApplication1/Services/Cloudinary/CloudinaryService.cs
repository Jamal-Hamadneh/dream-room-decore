using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using WebApplication1.Options;

namespace WebApplication1.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinaryOptions> options, IWebHostEnvironment environment)
    {
        var cloudinaryOptions = options.Value;
        var localEnv = LoadLocalEnv(Path.Combine(environment.ContentRootPath, ".env"));

        var cloudName = FirstValue(cloudinaryOptions.CloudName, localEnv, "CLOUDINARY_CLOUD_NAME");
        var apiKey = FirstValue(cloudinaryOptions.ApiKey, localEnv, "CLOUDINARY_API_KEY");
        var apiSecret = FirstValue(cloudinaryOptions.ApiSecret, localEnv, "CLOUDINARY_API_SECRET");

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    public async Task<string> UploadImageAsync(IFormFile image, string folder, CancellationToken cancellationToken = default)
    {
        await using var stream = image.OpenReadStream();
        var result = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(image.FileName, stream),
            Folder = folder
        }, cancellationToken);

        return result.SecureUrl?.ToString() ?? string.Empty;
    }

    public async Task<string> UploadImageStreamAsync(Stream imageStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var result = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(fileName, imageStream),
            Folder = folder
        }, cancellationToken);

        return result.SecureUrl?.ToString() ?? string.Empty;
    }

    public async Task<string> UploadImageFromUrlAsync(string imageUrl, string folder, CancellationToken cancellationToken = default)
    {
        var result = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(imageUrl),
            Folder = folder
        }, cancellationToken);

        return result.SecureUrl?.ToString() ?? imageUrl;
    }

    public async Task<string> UploadImageDataUriAsync(string imageDataUri, string folder, CancellationToken cancellationToken = default)
    {
        var result = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(imageDataUri),
            Folder = folder
        }, cancellationToken);

        return result.SecureUrl?.ToString() ?? string.Empty;
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
