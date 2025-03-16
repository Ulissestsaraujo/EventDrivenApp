using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class TestServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TestServiceScopeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceScope CreateScope()
    {
        return new TestServiceScope(_serviceProvider);
    }

    private class TestServiceScope : IServiceScope
    {
        public TestServiceScope(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose() { }
    }
}
