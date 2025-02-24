using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Api.Models;
using Api.Repositories;
using Newtonsoft.Json;
using Microsoft.AspNetCore.OData.Query.Validator;

namespace Api.Controllers.OData;

public interface ISampleProductsController
{
    IActionResult Get();
    Task<IActionResult> GetByKey(int key);
    Task<IActionResult> Post([FromBody] SampleProduct product);
    Task<IActionResult> Put(int key, [FromBody] SampleProduct product);
    Task<IActionResult> Delete(int key);
}

public class SampleProductsController : ODataController, ISampleProductsController
{
    private readonly ISampleProductRepository _repository;

    public SampleProductsController(ISampleProductRepository repository)
    {
        _repository = repository;
    }

    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All, PageSize = 2)]
    public IActionResult Get()
    {
        Console.WriteLine("\n=== ODATA REQUEST RECEIVED ===");
        
        try
        {
            // Get base query and ensure consistent ordering
            var query = _repository.GetAsQueryable().OrderBy(p => p.ProductId);
            
            // Log initial state
            var initialCount = query.Count();
            Console.WriteLine($"\nInitial Query State:");
            Console.WriteLine($"  Total Records: {initialCount}");
            
            // Log first few records
            var initialData = query.Take(5).ToList();
            Console.WriteLine("  First 5 Records:");
            foreach (var product in initialData)
            {
                Console.WriteLine($"    ID: {product.ProductId}, Name: {product.Name}, Price: {product.Price:C}");
            }
            
            return Ok(query);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing OData query: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return BadRequest($"Error processing OData query: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("=== END ODATA REQUEST ===\n");
        }
    }

    [EnableQuery]
    public async Task<IActionResult> GetByKey([FromRoute] int key)
    {
        var product = await _repository.GetByIdAsync(key);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    public async Task<IActionResult> Post([FromBody] SampleProduct product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _repository.AddAsync(product);
        return Created(result);
    }

    public async Task<IActionResult> Put([FromRoute] int key, [FromBody] SampleProduct product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (key != product.ProductId)
        {
            return BadRequest();
        }

        var result = await _repository.UpdateAsync(product);
        if (result == null)
        {
            return NotFound();
        }
        return Updated(result);
    }

    public async Task<IActionResult> Delete([FromRoute] int key)
    {
        var product = await _repository.GetByIdAsync(key);
        if (product == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(product);
        return NoContent();
    }
}
