using Microsoft.AspNetCore.Http;

namespace WebApplication1.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile image, string folder, CancellationToken cancellationToken = default);
    Task<string> UploadImageFromUrlAsync(string imageUrl, string folder, CancellationToken cancellationToken = default);
    Task<string> UploadImageDataUriAsync(string imageDataUri, string folder, CancellationToken cancellationToken = default);
}
