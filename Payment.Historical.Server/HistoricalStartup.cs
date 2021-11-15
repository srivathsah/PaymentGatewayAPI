using DataAccess;
using DistributedQueue;
using WebServerHostingUtils;

namespace Payment.Historical.Server;

public class HistoricalStartup : WebServerStartup
{
    public override async Task OnHostStarted()
    {
        await base.OnHostStarted();
        var configuration = Configuration;
        var connectionStringConfigurationSection = configuration?.GetSection("Historical");
        var connectionString = connectionStringConfigurationSection.GetValue<string>("ConnectionString");
        await connectionString.CreateSchema();
        IExternalEventsPublisher? externalEventsPublisher = Hostable?.Services.GetService<IExternalEventsPublisher>();
        if (externalEventsPublisher is not null)
            await externalEventsPublisher.ExecuteAsync(new CancellationTokenSource().Token);
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddSingleton<IHistoricalService, HistoricalService>();
        services.AddSingleton<IConnectionStringFactory, ConnectionStringFactory>();
        var rabbitMqConfigurationSection = Configuration?.GetSection("RabbitMq");
        string host = rabbitMqConfigurationSection.GetValue<string>(nameof(host));
        string queueName = rabbitMqConfigurationSection.GetValue<string>(nameof(queueName));
        services.AddDistributedQueue(host, queueName);
        services.AddSingleton<IExternalEventsPublisher, MqHostedService>();
    }
}
