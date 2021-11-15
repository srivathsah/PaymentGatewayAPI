using Backend.Shared;
using DataAccess;
using DistributedQueue;
using Domain.EventSourcing.DataAccess;
using EventBus;
using HostingUtils;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serialization;
using SqlStreamStore;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace ServerUtils;

public class OrleansServerStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        var config = new Config();
        Configuration.Bind("Config", config);
        services.AddSingleton(config);
        services.AddSingleton<IEventBus, RxEventBus>();
        services.AddHostedService<ConsulCleanup>();
        AddEventStore();
        AddDistributedQueue();

        void AddEventStore()
        {
            var eventStoreConfigurationSection = Configuration?.GetSection("EventStore");
            string eventStoreConnectionString = eventStoreConfigurationSection.GetValue<string>("ConnectionString");
            if (!string.IsNullOrWhiteSpace(eventStoreConnectionString))
            {
                services.AddSingleton<IEventDataAccess, StreamStoreEventsDataAccess>();
                services.AddSingleton<ISnapshotDataAccess, SnapshotsDataAccess>();
                services.AddSingleton<IInternalCommittedEventsPublisher, CommittedEventsPublisherGrain>();
                var pgDataAccess = new PostgresDataAccess(new Serializer());
                pgDataAccess.CreateDB(eventStoreConnectionString).Wait();
                var sqlSettings = new PostgresStreamStoreSettings(eventStoreConnectionString);
                var sqlStreamStore = new PostgresStreamStore(sqlSettings);
                sqlStreamStore.CreateSchemaIfNotExists().Wait();
                services.AddSingleton((IStreamStore)sqlStreamStore);
            }
        }

        void AddDistributedQueue()
        {
            var rabbitMqConfigurationSection = Configuration?.GetSection("RabbitMq");
            string host = rabbitMqConfigurationSection.GetValue<string>(nameof(host));
            string queueName = rabbitMqConfigurationSection.GetValue<string>(nameof(queueName));
            services.AddDistributedQueue(host, queueName);
        }
    }

    public override IHostBuilder BeforeHostBuild(IHostBuilder hostBuilder)
    {
        hostBuilder = base.BeforeHostBuild(hostBuilder);
        var configuration = Configuration;
        return hostBuilder
            .UseOrleans(builder =>
            {
                var orleansConfigurationSection = configuration?.GetSection("Orleans");
                string clusterId = orleansConfigurationSection.GetValue<string>(nameof(clusterId));
                string serviceId = Guid.NewGuid().ToString(); // orleansConfigurationSection.GetValue<string>(nameof(serviceId));
                    int siloPort = orleansConfigurationSection.GetValue<int>(nameof(siloPort));
                int gatewayPort = orleansConfigurationSection.GetValue<int>(nameof(gatewayPort));
                IPAddress siloAddress = IPAddress.Parse(orleansConfigurationSection.GetValue<string>(nameof(siloAddress)));
                string adonetConnectionString = orleansConfigurationSection.GetValue<string>(nameof(adonetConnectionString));

                async Task Startup(IServiceProvider serviceProvider, CancellationToken cancellationToken)
                {
                    IClusterClient? clusterClient = serviceProvider.GetService<IClusterClient>();
                    if (clusterClient is IClusterClient)
                        await clusterClient.GetGrain<IInternalCommittedEventsPublisher>("CommittedEventsPublisher").Start();
                    await OnOrleansStarted(serviceProvider, cancellationToken);
                }

                builder.AddStartupTask(Startup);
                ConfigureOrleans(builder, clusterId, serviceId, gatewayPort);
            });

        static void ConfigureOrleans(ISiloBuilder builder, string demoClusterId, string demoServiceId, int localhostGatewayPort) => builder
            .ConfigureApplicationParts(parts =>
            {
                parts.AddFromApplicationBaseDirectory();
            })
            //.UseDashboard(options => { })
            .AddSimpleMessageStreamProvider("ORLEANS_STREAM_PROVIDER")
            .AddMemoryGrainStorage("PubSubStore")
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = demoClusterId;
                options.ServiceId = demoServiceId;
            })
            .UseConsulClustering(gatewayOptions =>
            {
                gatewayOptions.Address = new Uri("http://localhost:8500");
                gatewayOptions.KvRootFolder = "test";
            })
            .Configure<ClusterMembershipOptions>(clusterOptions =>
            {
                clusterOptions.DefunctSiloCleanupPeriod = TimeSpan.FromSeconds(30);
                clusterOptions.DefunctSiloExpiration = TimeSpan.FromSeconds(30);
                clusterOptions.IAmAliveTablePublishTimeout = TimeSpan.FromMinutes(1);
                clusterOptions.NumMissedTableIAmAliveLimit = 2;
            })
            .ConfigureEndpoints(siloPort: GetAvailablePort(10000), gatewayPort: localhostGatewayPort)
            .ConfigureApplicationParts(x => x
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .WithReferences());



        static int GetAvailablePort(int startingPort)
        {
            var portArray = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            // Ignore active connections
            var connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // Ignore active tcp listners
            var endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // Ignore active UDP listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }
    }

    public virtual Task OnOrleansStarted(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
    public virtual ISiloBuilder BeforeOrleansBuild(ISiloBuilder siloBuilder) => siloBuilder;
}
