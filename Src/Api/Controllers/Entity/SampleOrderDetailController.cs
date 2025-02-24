namespace Api.Controllers.Entity;

[ApiController]
[Route("[controller]")]
public class SampleOrderDetailController : GenericController<SampleCompoundKeyOrderDetail, int>
{
    private readonly ISampleOrderDetailRepository _orderDetailRepository;

    public SampleOrderDetailController(ISampleOrderDetailRepository repository) : base(repository)
    {
        _orderDetailRepository = repository;
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<SampleCompoundKeyOrderDetail>>> GetOrderDetailsByOrder(int orderId)
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

    protected override int GetEntityId(SampleCompoundKeyOrderDetail entity)
    {
        return entity.OrderId;
    }
}
