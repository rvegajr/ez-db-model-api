using System.Linq.Expressions;

namespace Api.Infrastructure.Base;

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
        var propertyName = typeof(TEntity).Name switch
        {
            "SampleProduct" => "ProductId",
            "SampleOrder" => "OrderId",
            "SampleOrderDetail" => "OrderId",
            _ => "Id"
        };

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<TKey>(e, propertyName).Equals(id));
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty(typeof(TEntity).Name == "SampleProduct" ? "ProductId" : "OrderId")
            ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have an ID property.");

        var id = (TKey)idProperty.GetValue(entity)
            ?? throw new InvalidOperationException($"ID property of {typeof(TEntity).Name} is null.");

        var propertyName = typeof(TEntity).Name switch
        {
            "SampleProduct" => "ProductId",
            "SampleOrder" => "OrderId",
            "SampleOrderDetail" => "OrderId",
            _ => "Id"
        };

        var existingEntity = await _dbSet
            .AsTracking()
            .FirstOrDefaultAsync(e => EF.Property<TKey>(e, propertyName).Equals(id))
            ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} with ID {id} not found.");

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();

        // Detach the entity to avoid tracking issues
        _context.Entry(existingEntity).State = EntityState.Detached;

        // Get a fresh copy from the database
        var updatedEntity = await _dbSet
            .FirstOrDefaultAsync(e => EF.Property<TKey>(e, propertyName).Equals(id));

        return updatedEntity;
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        var idProperty = typeof(TEntity).GetProperty(typeof(TEntity).Name == "SampleProduct" ? "ProductId" : "OrderId")
            ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have an ID property.");

        var id = (TKey)idProperty.GetValue(entity)
            ?? throw new InvalidOperationException($"ID property of {typeof(TEntity).Name} is null.");

        var propertyName = typeof(TEntity).Name switch
        {
            "SampleProduct" => "ProductId",
            "SampleOrder" => "OrderId",
            "SampleOrderDetail" => "OrderId",
            _ => "Id"
        };

        var existingEntity = await _dbSet
            .AsTracking()
            .FirstOrDefaultAsync(e => EF.Property<TKey>(e, propertyName).Equals(id))
            ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} with ID {id} not found.");

        _dbSet.Remove(existingEntity);
        await _context.SaveChangesAsync();

        // Clear the context to ensure the entity is truly removed
        _context.ChangeTracker.Clear();
    }

    public virtual async Task<bool> ExistsAsync(TKey id)
    {
        return await GetByIdAsync(id) != null;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var entities = await _dbSet.AsNoTracking().ToListAsync();
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
