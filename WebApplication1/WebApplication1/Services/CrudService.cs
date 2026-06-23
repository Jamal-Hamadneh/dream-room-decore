using WebApplication1.Repositories;

namespace WebApplication1.Services;

public class CrudService<TEntity, TRequest, TResponse>(
    ICrudRepository<TEntity> repository,
    ICrudMapper<TEntity, TRequest, TResponse> mapper) : ICrudService<TRequest, TResponse>
    where TEntity : class
{
    protected ICrudRepository<TEntity> Repository { get; } = repository;
    protected ICrudMapper<TEntity, TRequest, TResponse> Mapper { get; } = mapper;

    public virtual async Task<List<TResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken);
        return entities.Select(Mapper.ToResponse).ToList();
    }

    public virtual async Task<TResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? default : Mapper.ToResponse(entity);
    }

    public virtual async Task<TResponse> CreateAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var entity = Mapper.ToEntity(request);
        SetPropertyIfExists(entity, "CreatedAt", DateTime.UtcNow, onlyWhenDefault: true);
        var created = await Repository.AddAsync(entity, cancellationToken);
        return Mapper.ToResponse(created);
    }

    public virtual async Task<TResponse?> UpdateAsync(int id, TRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return default;
        }

        Mapper.UpdateEntity(entity, request);
        SetPropertyIfExists(entity, "UpdatedAt", DateTime.UtcNow);
        var updated = await Repository.UpdateAsync(entity, cancellationToken);
        return Mapper.ToResponse(updated);
    }

    public virtual Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return Repository.DeleteAsync(id, cancellationToken);
    }

    private static void SetPropertyIfExists(TEntity entity, string propertyName, DateTime value, bool onlyWhenDefault = false)
    {
        var property = typeof(TEntity).GetProperty(propertyName);
        if (property is null || !property.CanWrite || property.PropertyType != typeof(DateTime) && property.PropertyType != typeof(DateTime?))
        {
            return;
        }

        if (onlyWhenDefault && property.GetValue(entity) is DateTime current && current != default)
        {
            return;
        }

        property.SetValue(entity, value);
    }
}
