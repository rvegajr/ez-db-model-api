using Xunit;

namespace Test;

[CollectionDefinition("TestCollection")]
public class TestCollection : ICollectionFixture<TestWebApplicationFactory<Program>>
{
    // This class has no code, and is never created. Its purpose is to be the place
    // to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}
