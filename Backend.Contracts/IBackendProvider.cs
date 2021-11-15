using Microsoft.Extensions.DependencyInjection;

namespace Backend.Contracts;

public interface IBackendProvider
{
    IServiceCollection AddBackend(IServiceCollection services);
    Task Initialize(IServiceProvider serviceProvider);
}
