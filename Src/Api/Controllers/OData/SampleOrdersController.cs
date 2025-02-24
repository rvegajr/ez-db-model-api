using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Api.Models;
using Api.Repositories;

namespace Api.Controllers.OData;

public class SampleOrdersController : ODataController
{
    private readonly ISampleOrderRepository _repository;

    public SampleOrdersController(ISampleOrderRepository repository)
    {
        _repository = repository;
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_repository.GetAsQueryable());
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var order = await _repository.GetByIdAsync(key);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    public async Task<IActionResult> Post([FromBody] SampleOrder order)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _repository.AddAsync(order);
        return Created(result);
    }

    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] SampleOrder order)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (key != order.OrderId)
        {
            return BadRequest();
        }

        var result = await _repository.UpdateAsync(order);
        if (result == null)
        {
            return NotFound();
        }
        return Updated(result);
    }

    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        var order = await _repository.GetByIdAsync(key);
        if (order == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(order);
        return NoContent();
    }
}
