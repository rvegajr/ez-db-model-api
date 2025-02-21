using System.Linq.Expressions;

namespace Api.Infrastructure.Base;

public interface IGenericRepository<TEntity, TKey> where TEntity : class
{
    IQueryable<TEntity> GetAsQueryable();
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity?> UpdateAsync(TEntity entity);
    Task<TEntity?> DeleteAsync(TKey id);
    Task<bool> ExistsAsync(TKey id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
}
