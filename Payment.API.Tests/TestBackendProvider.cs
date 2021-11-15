using Backend.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.API.Tests;

internal class TestBackendProvider : IBackendProvider
{
    public IServiceCollection AddBackend(IServiceCollection services)
    {
        throw new NotImplementedException();
    }

    public Task Initialize(IServiceProvider serviceProvider) => Task.CompletedTask;
}
