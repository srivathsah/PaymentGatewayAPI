using Backend.Contracts;
using Backend.Distributed;
using ClusterUtils;
using Serialization;
using WebServerHostingUtils;

namespace Payment.API;

public class ApiStartup : WebServerStartup
{
    public override async Task OnHostStarted()
    {
        await base.OnHostStarted();
        IAppInitializer? appInitializer = (Hostable?.Services.GetService(typeof(IAppInitializer)) as IAppInitializer);
        if (appInitializer is not null)
            await appInitializer.Initialise();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        IBackendProvider backendProvider = new DistributedBackendProvider(new ClusterClientFactory());
        services.AddSingleton(backendProvider);
        backendProvider.AddBackend(services);

        var historicalSection = Configuration?.GetSection("Historical");
        var historicalUrl = historicalSection.GetValue<string>("ServerUrl");
        var serializer = new Serializer();
        services.AddSingleton<ISerializer>(serializer);
        services.AddSingleton(serializer.JsonSerializerSettings);
        services.AddSingleton<IAppInitializer, Initializer>();
        services.AddSingleton<IMerchantService, MerchantService>();
        services.AddHttpClient("Historical", c =>
        {
            c.BaseAddress = new(historicalUrl);
        });
    }

    public override void AddEndPoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        base.AddEndPoints(endpointRouteBuilder);
        endpointRouteBuilder.MapControllers();
    }

    public override void AddAuthentication(IServiceCollection services)
    {
        base.AddAuthentication(services);
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "https://dev-2224837-admin.okta.com/oauth2/default";
                options.Audience = "api://default";
                options.RequireHttpsMetadata = false;
            });
    }

    public override void AddAuthorization(IServiceCollection services)
    {
        base.AddAuthorization(services);
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "api1");
            });
        });
    }
}
