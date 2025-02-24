using Xunit.Abstractions;

namespace Test;

public class SampleProductControllerTests : TestBase, IAsyncLifetime, IClassFixture<TestWebApplicationFactory<TestStartup>>
{
    private readonly ITestOutputHelper _output;

    public SampleProductControllerTests(
        TestWebApplicationFactory<Program> factory,
        ITestOutputHelper output) : base(factory)
    {
        _output = output;
        TestOutputHelper.Initialize(output);
    }

    public async Task InitializeAsync()
    {
        await _factory.SeedDatabase();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    async Task GetProducts_ReturnsAllProducts()
    {
        _output.WriteLine("\nTesting: Get All Products");
        _output.WriteLine("Checking if we can retrieve all products from the database");
        // Act
        var response = await _client.GetAsync("/SampleProduct");
        var products = await response.Content.ReadFromJsonAsync<List<SampleProduct>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        products.Should().NotBeNull();
        products.Should().HaveCount(3);
    }

    [Fact]
    async Task GetById_ReturnsProduct_WhenProductExists()
    {
        _output.WriteLine("\nTesting: Get Product By ID");
        _output.WriteLine("Checking if we can retrieve a specific product using its ID");
        // Act
        var response = await _client.GetAsync("/SampleProduct/1");
        var product = await response.Content.ReadFromJsonAsync<SampleProduct>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        product.Should().NotBeNull();
        product!.ProductId.Should().Be(1);
        product.Name.Should().Be("Test Product 1");
    }

    [Fact]
    async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        _output.WriteLine("\nTesting: Get Non-existent Product");
        _output.WriteLine("Checking if we get NotFound when requesting a product that doesn't exist");
        // Act
        var response = await _client.GetAsync("/SampleProduct/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    async Task CreateProduct_CreatesProduct_WhenModelIsValid()
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
        var response = await _client.PostAsJsonAsync("/SampleProduct", newProduct);
        var product = await response.Content.ReadFromJsonAsync<SampleProduct>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        product.Should().NotBeNull();
        product!.Name.Should().Be("New Product");
    }

    [Fact]
    async Task Update_UpdatesProduct_WhenProductExists()
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
        var response = await _client.PutAsJsonAsync("/SampleProduct/1", product);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedProduct = await GetContext().Products.FindAsync(1);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Updated Product");
    }

    [Fact]
    async Task Delete_DeletesProduct_WhenProductExists()
    {
        // Act
        var response = await _client.DeleteAsync("/SampleProduct/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var deletedProduct = await GetContext().Products.FindAsync(1);
        deletedProduct.Should().BeNull();
    }
}
