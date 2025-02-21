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

public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class
{
    protected readonly SampleDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(SampleDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual IQueryable<TEntity> GetAsQueryable() => _dbSet;

    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        if (typeof(TEntity) == typeof(SampleOrderDetail))
        {
            // For SampleOrderDetail, we need both OrderId and ProductId
            return await _dbSet.FirstOrDefaultAsync(e => EF.Property<int>(e, "OrderId").Equals(id));
        }

        var propertyName = typeof(TEntity).Name switch
        {
            "SampleProduct" => "ProductId",
            "SampleOrder" => "OrderId",
            _ => "Id"
        };

        return await _dbSet.FirstOrDefaultAsync(e => EF.Property<TKey>(e, propertyName).Equals(id));
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<TEntity?> UpdateAsync(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty(typeof(TEntity).Name == "SampleProduct" ? "ProductId" : "OrderId");
        if (idProperty == null) return null;

        var id = (TKey)idProperty.GetValue(entity);
        var existingEntity = await GetByIdAsync(id);
        if (existingEntity == null) return null;

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return existingEntity;
    }

    public virtual async Task<TEntity?> DeleteAsync(TKey id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return null;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> ExistsAsync(TKey id)
    {
        return await GetByIdAsync(id) != null;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var entities = await _dbSet.ToListAsync();
        return entities ?? Enumerable.Empty<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    protected virtual Expression<Func<TEntity, bool>> GetByIdPredicate(TKey id)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, constant);
        return Expression.Lambda<Func<TEntity, bool>>(equality, parameter);
    }
}
