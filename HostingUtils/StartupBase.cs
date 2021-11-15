using DataAccess;
using Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serialization;
using Serilog;
using ServerConfiguration;

namespace HostingUtils;

public class StartupBase
{
    public IConfiguration? Configuration { get; protected set; }
    public Action<ILoggingBuilder>? LoggingConfigurator { get; protected set; }
    public IServiceCollection? Services { get; protected set; }
    public IHostBuilder? HostBuilder { get; protected set; }
    public IHost? Hostable { get; protected set; }

    public virtual void ConfigureServices(IServiceCollection services)
    {
    }

    public virtual IHostBuilder GetHostBuilder() => Host.CreateDefaultBuilder();

    protected virtual IConfigurationBuilder BeforeConfigBuild(IConfigurationBuilder configurationBuilder) => configurationBuilder;

    public virtual IHost Build(string[] args)
    {
        HostBuilder = GetHostBuilder();
        HostBuilder = AddToHostBuilder(args, HostBuilder);
        Hostable = HostBuilder.Build();
        return Hostable;
    }

    public virtual IHostBuilder AddToHostBuilder(string[] args, IHostBuilder hostBuilder)
    {
        var configBuilder = args.GetConfigurationBuilder();
        configBuilder = BeforeConfigBuild(configBuilder);
        Configuration = configBuilder.Build();
        HostBuilder = hostBuilder
            .ConfigureAppConfiguration(builder =>
            {
                builder.Sources.Clear();
                builder.AddConfiguration(Configuration);
            })
            .ConfigureServices((hostContext, servicesCollection) =>
            {
                servicesCollection.AddLogging(loggingProvider => loggingProvider.AddFileLogger(@"C:\Temp\Logs\Log.log"));
                var serializer = new Serializer();
                servicesCollection.AddSingleton<ISerializer>(serializer);
                servicesCollection.AddSingleton(serializer.JsonSerializerSettings);
                servicesCollection.AddSingleton<IBaseDataAccess, PostgresDataAccess>();
                servicesCollection.AddSingleton<IConnectionStringFactory, ConnectionStringFactory>();
                ConfigureServices(servicesCollection);
            });

        HostBuilder = BeforeHostBuild(HostBuilder);
        return HostBuilder;
    }

    public virtual IHostBuilder BeforeHostBuild(IHostBuilder hostBuilder) => hostBuilder;
    public async Task RunHostAsync(CancellationToken cancellationToken, string[]? args = null)
    {
        if (Hostable == null)
            Hostable = Build(args ?? Array.Empty<string>());
        var services = Hostable.Services;
        var lifetime = services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() => OnHostStarted().Wait());
        lifetime.ApplicationStopped.Register(() => OnHostStopped().Wait());
        lifetime.ApplicationStopping.Register(() => OnHostStopping().Wait());
        await Hostable.RunAsync(cancellationToken);
    }
    public virtual Task OnHostStarted() => Task.CompletedTask;
    public virtual Task OnHostStopped()
    {
        Log.CloseAndFlush();
        return Task.CompletedTask;
    }

    public virtual Task OnHostStopping() => Task.CompletedTask;
}
