namespace Api.Infrastructure.Base;

public interface IGenericRepository<TEntity, TKey> where TEntity : class
{
    IQueryable<TEntity> GetAsQueryable();
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}
