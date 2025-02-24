using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Api.Models;
using Api.Repositories;

namespace Api.Controllers.OData;

public interface ISampleOrderDetailsController
{
    IActionResult Get();
    Task<IActionResult> GetByKey(int key);
    Task<IActionResult> Post([FromBody] SampleCompoundKeyOrderDetail orderDetail);
    Task<IActionResult> Put(int key, [FromBody] SampleCompoundKeyOrderDetail orderDetail);
    Task<IActionResult> Delete(int key);
}

public class SampleOrderDetailsController : ODataController, ISampleOrderDetailsController
{
    private readonly ISampleOrderDetailRepository _repository;

    public SampleOrderDetailsController(ISampleOrderDetailRepository repository)
    {
        _repository = repository;
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_repository.GetAsQueryable());
    }

    [EnableQuery]
    public async Task<IActionResult> GetByKey([FromRoute] int key)
    {
        var orderDetail = await _repository.GetByIdAsync(key);
        if (orderDetail == null)
        {
            return NotFound();
        }
        return Ok(orderDetail);
    }

    public async Task<IActionResult> Post([FromBody] SampleCompoundKeyOrderDetail orderDetail)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _repository.AddAsync(orderDetail);
        return Created(result);
    }

    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] SampleCompoundKeyOrderDetail orderDetail)
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
