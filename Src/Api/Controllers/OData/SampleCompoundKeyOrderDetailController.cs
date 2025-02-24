using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Api.Models;
using Api.Repositories;

namespace Api.Controllers.OData;

public interface ISampleCompoundKeyOrderDetailController
{
    IActionResult Get();
    Task<IActionResult> GetByKey(int orderId, int productId);
    Task<IActionResult> Post([FromBody] SampleCompoundKeyOrderDetail orderDetail);
    Task<IActionResult> Put(int orderId, int productId, [FromBody] SampleCompoundKeyOrderDetail orderDetail);
    Task<IActionResult> Delete(int orderId, int productId);
}

public class SampleCompoundKeyOrderDetailController : ODataController, ISampleCompoundKeyOrderDetailController
{
    private readonly ISampleCompoundKeyOrderDetailRepository _repository;

    public SampleCompoundKeyOrderDetailController(ISampleCompoundKeyOrderDetailRepository repository)
    {
        _repository = repository;
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_repository.GetAsQueryable());
    }

    [EnableQuery]
    public async Task<IActionResult> GetByKey([FromRoute] int orderId, [FromRoute] int productId)
    {
        var orderDetail = await _repository.GetByCompoundKeyAsync(orderId, productId);
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

    public async Task<IActionResult> Put([FromRoute] int orderId, [FromRoute] int productId, [FromBody] SampleCompoundKeyOrderDetail orderDetail)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (orderId != orderDetail.OrderId || productId != orderDetail.ProductId)
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

    public async Task<IActionResult> Delete([FromRoute] int orderId, [FromRoute] int productId)
    {
        var orderDetail = await _repository.GetByCompoundKeyAsync(orderId, productId);
        if (orderDetail == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(orderDetail);
        return NoContent();
    }
}
