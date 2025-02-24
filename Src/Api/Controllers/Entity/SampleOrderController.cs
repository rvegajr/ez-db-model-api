namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SampleOrderController : GenericController<SampleOrder, int>
{
    private readonly ISampleOrderRepository _orderRepository;

    public SampleOrderController(ISampleOrderRepository repository) : base(repository)
    {
        _orderRepository = repository;
    }

    [HttpGet("customer/{customerName}")]
    public async Task<ActionResult<IEnumerable<SampleOrder>>> GetOrdersByCustomer(string customerName)
    {
        var orders = await _orderRepository.GetOrdersByCustomerAsync(customerName);
        return Ok(orders);
    }

    [HttpGet("{id}/total")]
    public async Task<ActionResult<decimal>> GetOrderTotal(int id)
    {
        var total = await _orderRepository.GetTotalOrderValueAsync(id);
        return Ok(total);
    }

    public override async Task<ActionResult<SampleOrder>> Create([FromBody] SampleOrder order)
    {
        // Calculate total amount based on order details
        order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        return await base.Create(order);
    }

    public override async Task<IActionResult> Update(int id, [FromBody] SampleOrder order)
    {
        // Recalculate total amount
        order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        return await base.Update(id, order);
    }
}
