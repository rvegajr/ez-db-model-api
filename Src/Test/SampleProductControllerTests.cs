using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Controllers;
using Api.Data;
using Api.Models;
using Xunit;

namespace Test;

public class SampleProductControllerTests
{
    private readonly SampleDbContext _context;
    private readonly SampleProductController _controller;

    public SampleProductControllerTests()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new SampleDbContext(options);
        _controller = new SampleProductController(_context);

        // Seed test data
        var product = new SampleProduct
        {
            Id = 1,
            Name = "Test Product",
            Price = 19.99m,
            Description = "Test Description"
        };
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        // Act
        var result = await _controller.GetProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<SampleProduct>>>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<SampleProduct>>(actionResult.Value);
        Assert.Single(products);
    }

    [Fact]
    public async Task GetProduct_ReturnsProduct_WhenProductExists()
    {
        // Act
        var result = await _controller.GetProduct(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleProduct>>(result);
        var product = Assert.IsType<SampleProduct>(actionResult.Value);
        Assert.Equal(1, product.Id);
        Assert.Equal("Test Product", product.Name);
    }

    [Fact]
    public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Act
        var result = await _controller.GetProduct(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleProduct>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task CreateProduct_CreatesProduct_WhenModelIsValid()
    {
        // Arrange
        var newProduct = new SampleProduct
        {
            Name = "New Product",
            Price = 29.99m,
            Description = "New Description"
        };

        // Act
        var result = await _controller.CreateProduct(newProduct);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleProduct>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var product = Assert.IsType<SampleProduct>(createdAtActionResult.Value);
        Assert.Equal("New Product", product.Name);
    }

    [Fact]
    public async Task UpdateProduct_UpdatesProduct_WhenProductExists()
    {
        // Arrange
        var product = new SampleProduct
        {
            Id = 1,
            Name = "Updated Product",
            Price = 39.99m,
            Description = "Updated Description"
        };

        // Act
        var result = await _controller.UpdateProduct(1, product);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedProduct = await _context.Products.FindAsync(1);
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Product", updatedProduct.Name);
    }

    [Fact]
    public async Task DeleteProduct_DeletesProduct_WhenProductExists()
    {
        // Act
        var result = await _controller.DeleteProduct(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var deletedProduct = await _context.Products.FindAsync(1);
        Assert.Null(deletedProduct);
    }
}
