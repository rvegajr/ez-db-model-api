using Xunit.Abstractions;

namespace Test;

public class CachingTests : TestBase, IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    public CachingTests(
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
    public async Task GetProducts_CachesResponse()
    {
        // Arrange
        _output.WriteLine("\nTesting: Caching of Get All Products");

        // Act 1 - First request
        var response1 = await _client.GetAsync("/SimpleSampleProduct");
        var products1 = await response1.Content.ReadFromJsonAsync<List<SampleProduct>>();

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        products1.Should().NotBeNull();
        products1.Should().HaveCount(3);

        // Act 2 - Second request (should be from cache)
        var response2 = await _client.GetAsync("/SimpleSampleProduct");
        var products2 = await response2.Content.ReadFromJsonAsync<List<SampleProduct>>();

        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        products2.Should().NotBeNull();
        products2.Should().HaveCount(3);

        // Assert - both responses should be identical
        JsonConvert.SerializeObject(products1)
            .Should().Be(JsonConvert.SerializeObject(products2));
    }

    [Fact]
    public async Task GetProduct_CachesResponse()
    {
        // Arrange
        _output.WriteLine("\nTesting: Caching of Get Single Product");

        // Act 1 - First request
        var firstProduct = GetContext().Products.First();
        var response1 = await _client.GetAsync($"/SimpleSampleProduct/{firstProduct.ProductId}");
        var product1 = await response1.Content.ReadFromJsonAsync<SampleProduct>();

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        product1.Should().NotBeNull();
        product1.Should().NotBeNull();

        // Act 2 - Second request (should be from cache)
        var response2 = await _client.GetAsync($"/SimpleSampleProduct/{firstProduct.ProductId}");
        var product2 = await response2.Content.ReadFromJsonAsync<SampleProduct>();

        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        product2.Should().NotBeNull();
        product2.Should().NotBeNull();

        // Assert - both responses should be identical
        JsonConvert.SerializeObject(product1)
            .Should().Be(JsonConvert.SerializeObject(product2));
    }

    [Fact]
    public async Task GetProduct_DifferentIds_DifferentResponses()
    {
        // Arrange
        _output.WriteLine("\nTesting: Different Products Have Different Cache Keys");

        var products = GetContext().Products.OrderBy(p => p.ProductId).Take(2).ToList();

        // Act - Get first product twice
        var response1a = await _client.GetAsync($"/SimpleSampleProduct/{products[0].ProductId}");
        var response1b = await _client.GetAsync($"/SimpleSampleProduct/{products[0].ProductId}");
        var product1a = await response1a.Content.ReadFromJsonAsync<SampleProduct>();
        var product1b = await response1b.Content.ReadFromJsonAsync<SampleProduct>();

        // Get second product twice
        var response2a = await _client.GetAsync($"/SimpleSampleProduct/{products[1].ProductId}");
        var response2b = await _client.GetAsync($"/SimpleSampleProduct/{products[1].ProductId}");
        var product2a = await response2a.Content.ReadFromJsonAsync<SampleProduct>();
        var product2b = await response2b.Content.ReadFromJsonAsync<SampleProduct>();

        // Assert
        // Same product should return same response
        JsonConvert.SerializeObject(product1a)
            .Should().Be(JsonConvert.SerializeObject(product1b));
        JsonConvert.SerializeObject(product2a)
            .Should().Be(JsonConvert.SerializeObject(product2b));

        // Different products should return different responses
        JsonConvert.SerializeObject(product1a)
            .Should().NotBe(JsonConvert.SerializeObject(product2a));
    }
}
