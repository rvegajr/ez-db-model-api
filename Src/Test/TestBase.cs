namespace Test;

public abstract class TestBase : IDisposable
{
    protected readonly SampleDbContext _context;

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SampleDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
