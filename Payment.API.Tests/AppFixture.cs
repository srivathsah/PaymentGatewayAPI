using Alba;
using Backend.Contracts;
using Backend.Distributed;
using ClusterUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.API.Controllers;
using System.Security.Claims;

namespace Payment.API.Tests;

public class AppFixture
{
    public AppFixture()
    {
        var args = Array.Empty<string>();
        var startup = new TestApiGatewayStartup();
        var builder = startup.GetHostBuilder();
        builder = startup.AddToHostBuilder(args, builder);
        AlbaHost = builder.StartAlba();

        AlbaHost.BeforeEach(httpContext =>
        {
            var claims = new List<Claim>()
            {
                    new Claim("Merchant_Id", "1"),
                    new Claim("user_id", "s@g.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            httpContext.User = claimsPrincipal;
        });

        AlbaHost.AfterEach(httpContext =>
        {
        });

        AlbaHost.Services.GetService<IAppInitializer>()?.Initialise().Wait();
    }

    public readonly IAlbaHost AlbaHost;

    public void Dispose()
    {
        AlbaHost?.Dispose();
    }
}

class TestApiGatewayStartup : ApiStartup
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        FakeClusterClientFactory clusterClientFactory = new();
        services.AddSingleton<IClusterClientFactory>(clusterClientFactory);
        IBackendProvider backendProvider = new DistributedBackendProvider(clusterClientFactory);
        services.AddSingleton(backendProvider);
        backendProvider.AddBackend(services);
        services.AddSingleton<IMerchantService>(FakeClusterClientFactory.MerchantServiceMock.Object);
    }

    public override IHostBuilder BeforeHostBuild(IHostBuilder hostBuilder)
    {
        hostBuilder = base.BeforeHostBuild(hostBuilder);
        hostBuilder
            //.UseContentRoot(AlbaHost.DirectoryFinder.FindParallelFolder("WebApplication"))
            .UseEnvironment("Testing");
        return hostBuilder;
    }

    public override void BeforeMvcBuild(IMvcBuilder mvcBuilder)
    {
        base.BeforeMvcBuild(mvcBuilder);
        mvcBuilder.AddApplicationPart(typeof(PaymentGatewayController).Assembly);
    }
}
