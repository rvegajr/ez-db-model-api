using Xunit.Abstractions;

namespace Test;

public class SampleProductControllerTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly ISampleProductRepository _repository;
    private readonly SampleProductController _controller;

    public SampleProductControllerTests(ITestOutputHelper output)
    {
        _output = output;
        TestOutputHelper.Initialize(output);
        _repository = new SampleProductRepository(_context);
        _controller = new SampleProductController(_repository);

        // Seed test data
        var product = new SampleProduct
        {
            ProductId = 1,
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
        _output.WriteLine("\nTesting: Get All Products");
        _output.WriteLine("Checking if we can retrieve all products from the database");
        // Act
        var result = await _controller.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<SampleProduct>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<SampleProduct>>(okResult.Value);
        Assert.Single(products);
    }

    [Fact]
    public async Task GetById_ReturnsProduct_WhenProductExists()
    {
        _output.WriteLine("\nTesting: Get Product By ID");
        _output.WriteLine("Checking if we can retrieve a specific product using its ID");
        // Act
        var result = await _controller.GetById(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleProduct>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var product = Assert.IsType<SampleProduct>(okResult.Value);
        Assert.Equal(1, product.ProductId);
        Assert.Equal("Test Product", product.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        _output.WriteLine("\nTesting: Get Non-existent Product");
        _output.WriteLine("Checking if we get NotFound when requesting a product that doesn't exist");
        // Act
        var result = await _controller.GetById(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleProduct>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task CreateProduct_CreatesProduct_WhenModelIsValid()
    {
        _output.WriteLine("\nTesting: Create New Product");
        _output.WriteLine("Checking if we can create a new product with valid data");
        // Arrange
        var newProduct = new SampleProduct
        {
            Name = "New Product",
            Price = 29.99m,
            Description = "New Description"
        };

        // Act
        var result = await _controller.Create(newProduct);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SampleProduct>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var product = Assert.IsType<SampleProduct>(createdAtActionResult.Value);
        Assert.Equal("New Product", product.Name);
    }

    [Fact]
    public async Task Update_UpdatesProduct_WhenProductExists()
    {
        _output.WriteLine("\nTesting: Update Existing Product");
        _output.WriteLine("Checking if we can update an existing product's details");
        // Arrange
        var product = new SampleProduct
        {
            ProductId = 1,
            Name = "Updated Product",
            Price = 39.99m,
            Description = "Updated Description"
        };

        // Act
        var result = await _controller.Update(1, product);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedProduct = await _context.Products.FindAsync(1);
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Product", updatedProduct.Name);
    }

    [Fact]
    public async Task Delete_DeletesProduct_WhenProductExists()
    {
        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var deletedProduct = await _context.Products.FindAsync(1);
        Assert.Null(deletedProduct);
    }
}
