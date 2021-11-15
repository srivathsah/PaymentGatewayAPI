using Backend.Contracts;

namespace Payment.API;

public class Initializer : IAppInitializer
{
    private readonly IBackendProvider _backendProvider;
    private readonly IServiceProvider _serviceProvider;

    public Initializer(IBackendProvider backendProvider, IServiceProvider serviceProvider)
    {
        _backendProvider = backendProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task Initialise() => await _backendProvider.Initialize(_serviceProvider);
}
