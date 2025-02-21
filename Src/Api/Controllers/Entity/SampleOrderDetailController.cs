using Microsoft.AspNetCore.Mvc;
using Api.Infrastructure.Base;
using Api.Models;
using Api.Repositories;

namespace Api.Controllers.Entity;

[ApiController]
[Route("[controller]")]
public class SampleOrderDetailController : GenericController<SampleOrderDetail, int>
{
    private readonly IOrderDetailRepository _orderDetailRepository;

    public SampleOrderDetailController(IOrderDetailRepository repository) : base(repository)
    {
        _orderDetailRepository = repository;
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<SampleOrderDetail>>> GetOrderDetailsByOrder(int orderId)
    {
        var orderDetails = await _orderDetailRepository.GetOrderDetailsByOrderAsync(orderId);
        return Ok(orderDetails);
    }

    [HttpGet("{id}/total")]
    public async Task<ActionResult<decimal>> GetOrderDetailTotal(int id)
    {
        var total = await _orderDetailRepository.GetOrderDetailTotalAsync(id);
        return Ok(total);
    }

    protected override int GetEntityId(SampleOrderDetail entity)
    {
        return entity.OrderDetailId;
    }
}
