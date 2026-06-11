namespace WebApplication1.Services;

public interface ICrudMapper<TEntity, TRequest, TResponse>
{
    TEntity ToEntity(TRequest request);
    void UpdateEntity(TEntity entity, TRequest request);
    TResponse ToResponse(TEntity entity);
}
