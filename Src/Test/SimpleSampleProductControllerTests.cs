using Api.Controllers.Entity;
using Api.Infrastructure.Base;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Test;

public class SimpleSampleProductControllerTests : TestBase
{
    private readonly SimpleSampleProductController _controller;
    private readonly IGenericRepository<SampleProduct, int> _repository;

    public SimpleSampleProductControllerTests()
    {
        _repository = new GenericRepository<SampleProduct, int>(_context);
        _controller = new SimpleSampleProductController(_repository);
    }

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<SampleProduct>
        {
            new() { ProductId = 1, Name = "Product 1", Price = 10.99m },
            new() { ProductId = 2, Name = "Product 2", Price = 20.99m }
        };
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<SampleProduct>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
    }

    [Fact]
    public async Task GetById_ReturnsProduct_WhenExists()
    {
        // Arrange
        var product = new SampleProduct { ProductId = 1, Name = "Test Product", Price = 15.99m };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<SampleProduct>(okResult.Value);
        Assert.Equal("Test Product", returnedProduct.Name);
    }

    [Fact]
    public async Task Create_ReturnsCreatedProduct()
    {
        // Arrange
        var product = new SampleProduct { Name = "New Product", Price = 25.99m };

        // Act
        var result = await _controller.Create(product);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedProduct = Assert.IsType<SampleProduct>(createdResult.Value);
        Assert.Equal("New Product", returnedProduct.Name);
        Assert.NotEqual(0, returnedProduct.ProductId);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var product = new SampleProduct { ProductId = 1, Name = "Original Name", Price = 10.99m };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var updatedProduct = new SampleProduct { ProductId = 1, Name = "Updated Name", Price = 15.99m };

        // Act
        var result = await _controller.Update(1, updatedProduct);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var dbProduct = await _context.Products.FindAsync(1);
        Assert.Equal("Updated Name", dbProduct?.Name);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var product = new SampleProduct { ProductId = 1, Name = "Test Product", Price = 10.99m };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var dbProduct = await _context.Products.FindAsync(1);
        Assert.Null(dbProduct);
    }
}
