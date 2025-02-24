using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Api.Models;
using Api.Repositories;

namespace Api.Controllers.OData;

public class SampleOrderDetailsController : ODataController
{
    private readonly ISampleOrderDetailRepository _repository;

    public SampleOrderDetailsController(ISampleOrderDetailRepository repository)
    {
        _repository = repository;
    }

    [EnableQuery]
    public async Task<IActionResult> Get()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [EnableQuery]
    public async Task<IActionResult> Get([FromRoute] int key)
    {
        var orderDetail = await _repository.GetByIdAsync(key);
        if (orderDetail == null)
        {
            return NotFound();
        }
        return Ok(orderDetail);
    }

    public async Task<IActionResult> Post([FromBody] SampleOrderDetail orderDetail)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _repository.AddAsync(orderDetail);
        return Created(result);
    }

    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] SampleOrderDetail orderDetail)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (key != orderDetail.OrderId)
        {
            return BadRequest();
        }

        var result = await _repository.UpdateAsync(orderDetail);
        if (result == null)
        {
            return NotFound();
        }
        return Updated(result);
    }

    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        var orderDetail = await _repository.GetByIdAsync(key);
        if (orderDetail == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(orderDetail);
        return NoContent();
    }
}
