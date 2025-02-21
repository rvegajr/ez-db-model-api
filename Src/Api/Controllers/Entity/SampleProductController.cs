using Microsoft.AspNetCore.Mvc;
using Api.Infrastructure.Base;
using Api.Models;
using Api.Repositories;

namespace Api.Controllers.Entity;

[ApiController]
[Route("[controller]")]
public class SampleProductController : GenericController<SampleProduct, int>
{
    private readonly IProductRepository _productRepository;

    public SampleProductController(IProductRepository repository) : base(repository)
    {
        _productRepository = repository;
    }

    [HttpGet("price-range")]
    public async Task<ActionResult<IEnumerable<SampleProduct>>> GetProductsByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        var products = await _productRepository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
        return Ok(products);
    }
