namespace Api.Infrastructure.Base;

[ApiController]
public abstract class GenericController<TEntity, TKey> : ControllerBase
    where TEntity : class
{
    protected readonly IGenericRepository<TEntity, TKey> _repository;

    protected GenericController(IGenericRepository<TEntity, TKey> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [Cache(timeToLiveSeconds: 300, cacheType: ResponseCacheType.Public)]
    public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        if (entities == null || !entities.Any())
            return NotFound();
        return Ok(entities);
    }

    [HttpGet("{id}")]
    [Cache(timeToLiveSeconds: 300, cacheType: ResponseCacheType.Public)]
    public virtual async Task<ActionResult<TEntity>> GetById(TKey id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntity entity)
    {
        var result = await _repository.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = GetEntityId(result) }, result);
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update(TKey id, [FromBody] TEntity entity)
    {
        if (!EqualityComparer<TKey>.Default.Equals(id, GetEntityId(entity)))
            return BadRequest();

        var result = await _repository.UpdateAsync(entity);
        if (result == null) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(TKey id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result == null) return NotFound();
        return NoContent();
    }

    protected virtual TKey GetEntityId(TEntity entity)
    {
        var propertyName = typeof(TEntity).Name switch
        {
            "SampleProduct" => "ProductId",
            "SampleOrder" => "OrderId",
            "SampleOrderDetail" => "OrderId",
            _ => "Id"
        };

        var property = typeof(TEntity).GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have a {propertyName} property.");
        
        var value = property.GetValue(entity)
            ?? throw new InvalidOperationException($"{propertyName} property of {typeof(TEntity).Name} is null.");
            
        return (TKey)value;
    }
}
