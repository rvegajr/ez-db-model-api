using Microsoft.AspNetCore.Mvc;

namespace Test;

[Collection("TestCollection")]
public class SimpleSampleProductControllerTests : TestBase, IAsyncLifetime
{
    public SimpleSampleProductControllerTests(
        TestWebApplicationFactory<Program> factory) : base(factory)
    {
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
    async Task GetAll_ReturnsAllProducts()
    {
        // Arrange
        // Database is seeded in InitializeAsync

        // Act
        var response = await _client.GetAsync("/SimpleSampleProduct");
        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedProducts = JsonConvert.DeserializeObject<List<SampleProduct>>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedProducts.Should().NotBeNull();
        returnedProducts.Should().HaveCount(3);
    }

    [Fact]
    async Task GetById_ReturnsProduct_WhenExists()
    {
        // Arrange
        // Database is seeded in InitializeAsync

        // Act
        var firstProduct = GetContext().Products.First();
        var response = await _client.GetAsync($"/SimpleSampleProduct/{firstProduct.ProductId}");
        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedProduct = JsonConvert.DeserializeObject<SampleProduct>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedProduct.Should().NotBeNull();
        returnedProduct!.Name.Should().Be(firstProduct.Name);
        returnedProduct.Price.Should().Be(firstProduct.Price);
        returnedProduct.Description.Should().Be(firstProduct.Description);
    }

    [Fact]
    async Task Create_ReturnsCreatedProduct()
    {
        // Arrange
        var product = new SampleProduct { Name = "New Product", Price = 25.99m };

        // Act
        var json = JsonConvert.SerializeObject(product);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/SimpleSampleProduct", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedProduct = JsonConvert.DeserializeObject<SampleProduct>(responseContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        returnedProduct.Should().NotBeNull();
        returnedProduct!.Name.Should().Be("New Product");
        returnedProduct.ProductId.Should().NotBe(0);
    }

    [Fact]
    async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        // Database is seeded in InitializeAsync

        var firstProduct = GetContext().Products.First();
        var updatedProduct = new SampleProduct 
        { 
            ProductId = firstProduct.ProductId, 
            Name = "Updated Name", 
            Price = 15.99m,
            Description = "Updated Description"
        };

        // Act
        var json = JsonConvert.SerializeObject(updatedProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/SimpleSampleProduct/{firstProduct.ProductId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var dbProduct = await GetContext().Products.FindAsync(firstProduct.ProductId);
        dbProduct.Should().NotBeNull();
        dbProduct!.Name.Should().Be(updatedProduct.Name);
        dbProduct.Price.Should().Be(updatedProduct.Price);
        dbProduct.Description.Should().Be(updatedProduct.Description);
    }

    [Fact]
    async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        // Database is seeded in InitializeAsync
        var firstProduct = GetContext().Products.First();

        // Act
        var response = await _client.DeleteAsync($"/SimpleSampleProduct/{firstProduct.ProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var dbProduct = await GetContext().Products.FindAsync(firstProduct.ProductId);
        dbProduct.Should().BeNull();
    }
}
