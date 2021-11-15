using Backend.Shared;
using Domain.EventSourcing.DataAccess;
using EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Hosting;
using Orleans.TestingHost;
using Payment.Client;
using Payment.Contracts;
using Serialization;
using ServerUtils;
using SqlStreamStore;

[assembly: KnownAssembly(typeof(PaymentGatewayCommands))]
namespace Payment.Server.Tests;

public class TestSiloBuilderConfigurator : ISiloConfigurator, IHostConfigurator
{
    public static readonly Mock<IConfiguration> MockConfiguration = new(MockBehavior.Strict);
    public static readonly Mock<IEventDataAccess> MockEventsDataAccess = new(MockBehavior.Strict);
    public static readonly Mock<ISnapshotDataAccess> MockSnapshotDataAccess = new(MockBehavior.Strict);
    public static readonly Mock<IInternalCommittedEventsPublisher> MockCommittedEventsPublisher = new(MockBehavior.Strict);
    public static readonly Mock<IStreamStore> MockStreamStore = new(MockBehavior.Strict);

    static TestSiloBuilderConfigurator() =>
        MockEventsDataAccess.Setup(x => x.FetchHistoricalEvents(It.IsAny<object>(), It.IsAny<Action<object>>())).Returns(Task.CompletedTask);

    public static void SetupSnapshotDataAccess<T>() =>
        MockSnapshotDataAccess.Setup(x => x.GetSnapshot<T>(It.IsAny<string>())).Returns(Task.FromResult<T?>(default));
    public void Configure(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            var configuration = MockConfiguration.Object;
            MockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "ConnectionStrings:default")]).Returns("mock value");

            var config = new Config();
            services.AddSingleton(config);
            services.AddSingleton<IEventBus, RxEventBus>();
            services.AddSingleton(MockEventsDataAccess.Object);
            services.AddSingleton(MockSnapshotDataAccess.Object);
            services.AddSingleton(MockCommittedEventsPublisher.Object);
            services.AddSingleton(MockStreamStore.Object);
            services.AddSingleton<ISerializer, Serializer>();
        });
    }

    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureApplicationParts(x =>
        {
            x.AddApplicationPart(typeof(PaymentGatewayGrain).Assembly);
            x.AddApplicationPart(typeof(IPaymentGatewayClient).Assembly);
            x.AddApplicationPart(typeof(PaymentGatewayCommands).Assembly);
        })
        .AddMemoryGrainStorageAsDefault();
    }
}
