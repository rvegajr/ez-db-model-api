namespace Api.Controllers.Entity;

[ApiController]
[Route("[controller]")]
public class SampleCompoundKeyOrderDetailController : ControllerBase
{
    private readonly ISampleCompoundKeyOrderDetailRepository _repository;

    public SampleCompoundKeyOrderDetailController(ISampleCompoundKeyOrderDetailRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SampleCompoundKeyOrderDetail>>> GetAll()
    {
        var items = await _repository.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{orderId}/{productId}")]
    public async Task<ActionResult<SampleCompoundKeyOrderDetail>> GetById(int orderId, int productId)
    {
        var item = await _repository.GetByCompoundKeyAsync(orderId, productId);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<SampleCompoundKeyOrderDetail>>> GetOrderDetailsByOrder(int orderId)
    {
        var orderDetails = await _repository.GetOrderDetailsByOrderAsync(orderId);
        return Ok(orderDetails);
    }

    [HttpPost]
    public async Task<ActionResult<SampleCompoundKeyOrderDetail>> Create([FromBody] SampleCompoundKeyOrderDetail orderDetail)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if order exists
        if (!await _repository.OrderExistsAsync(orderDetail.OrderId))
        {
            return BadRequest(new { Error = $"Order {orderDetail.OrderId} not found" });
        }

        // Check if product exists
        if (!await _repository.ProductExistsAsync(orderDetail.ProductId))
        {
            return BadRequest(new { Error = $"Product {orderDetail.ProductId} not found" });
        }

        // Ensure navigation properties are null before saving
        orderDetail.Order = null;
        orderDetail.Product = null;

        var result = await _repository.AddAsync(orderDetail);
        return CreatedAtAction(nameof(GetById), new { orderId = result.OrderId, productId = result.ProductId }, result);
    }

    [HttpPut("{orderId}/{productId}")]
    public async Task<IActionResult> Update(int orderId, int productId, [FromBody] SampleCompoundKeyOrderDetail orderDetail)
    {
        if (orderId != orderDetail.OrderId || productId != orderDetail.ProductId)
        {
            return BadRequest(new { Error = "OrderId and ProductId in URL must match body" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if order exists
        if (!await _repository.OrderExistsAsync(orderDetail.OrderId))
        {
            return BadRequest(new { Error = $"Order {orderDetail.OrderId} not found" });
        }

        // Check if product exists
        if (!await _repository.ProductExistsAsync(orderDetail.ProductId))
        {
            return BadRequest(new { Error = $"Product {orderDetail.ProductId} not found" });
        }

        // Ensure navigation properties are null before saving
        orderDetail.Order = null;
        orderDetail.Product = null;

        var updated = await _repository.UpdateAsync(orderDetail);
        if (updated == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{orderId}/{productId}")]
    public async Task<IActionResult> Delete(int orderId, int productId)
    {
        var item = await _repository.GetByCompoundKeyAsync(orderId, productId);
        if (item == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(item);
        return NoContent();
    }

    [HttpGet("{orderId}/{productId}/total")]
    public async Task<ActionResult<decimal>> GetOrderDetailTotal(int orderId, int productId)
    {
        var total = await _repository.GetOrderDetailTotalAsync(orderId);
        return Ok(total);
    }


}
