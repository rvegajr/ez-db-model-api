namespace Api.Controllers.Entity;

/// <summary>
/// Example of a controller that needs custom functionality beyond CRUD.
/// Uses a custom repository interface and implementation.
/// </summary>
[ApiController]
[Route("[controller]")]
public class AdvancedSampleOrderController : GenericController<SampleOrder, int>
{
    private readonly IAdvancedSampleOrderRepository _orderRepository;

    public AdvancedSampleOrderController(IAdvancedSampleOrderRepository repository) 
        : base(repository)
    {
        _orderRepository = repository;
    }

    [HttpGet("customer/{customerName}")]
    public async Task<ActionResult<IEnumerable<SampleOrder>>> GetOrdersByCustomer(string customerName)
    {
        var orders = await _orderRepository.GetOrdersByCustomerAsync(customerName);
        return Ok(orders);
    }

    [HttpGet("stats/monthly")]
    public async Task<ActionResult<OrderStatistics>> GetMonthlyStatistics()
    {
        var stats = await _orderRepository.GetMonthlyStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("stats/customer/{customerId}")]
    public async Task<ActionResult<CustomerOrderSummary>> GetCustomerSummary(int customerId)
    {
        var summary = await _orderRepository.GetCustomerOrderSummaryAsync(customerId);
        return Ok(summary);
    }

    public override async Task<ActionResult<SampleOrder>> Create([FromBody] SampleOrder order)
    {
        // Add custom validation
        if (!await _orderRepository.ValidateOrderAsync(order))
        {
            return BadRequest("Invalid order");
        }

        // Add custom business logic
        await _orderRepository.ProcessNewOrderAsync(order);

        return await base.Create(order);
    }
}
