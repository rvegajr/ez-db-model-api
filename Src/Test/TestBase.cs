using Api.Infrastructure.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Xunit;

namespace Test;

[Collection("TestCollection")]
public abstract class TestBase : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly TestWebApplicationFactory<Program> _factory;

    protected TestBase(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _serviceProvider = factory.Services;
    }

    private IServiceScope? _scope;
    private SampleDbContext? _context;

    protected SampleDbContext GetContext()
    {
        if (_context == null)
        {
            _scope = _serviceProvider.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        }
        return _context;
    }

    public void Dispose()
    {
        _context?.Dispose();
        _scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}
