namespace WebApplication1.Services;

public interface ICrudService<TRequest, TResponse>
{
    Task<List<TResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TResponse> CreateAsync(TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> UpdateAsync(int id, TRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
