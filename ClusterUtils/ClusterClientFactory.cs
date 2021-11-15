using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Polly;

namespace ClusterUtils;

public class ClusterClientFactory : IClusterClientFactory
{
    public async Task<IClusterClient> GetClusterClient(int clusterPort, string cluster, string service, Action<ClientBuilder>? builderAction = null)
    {
        return await ExecuteAndRetry(cluster, service, "Host=localhost;Port=5432;Database=Cluster;User Id=postgres;Password=asdfsdf12;", builderAction).ConfigureAwait(false);
    }

    private static async Task<IClusterClient> ExecuteAndRetry(string clusterName, string serviceId, string adoNetConnectionstring, Action<ClientBuilder>? builderAction)
    {
        return await Policy
             .Handle<Exception>()
             .WaitAndRetryAsync(50, i => TimeSpan.FromSeconds(5), (result, timeSpan, retryCount, context) =>
             {
                     //Logger.Log.Warning($"Connection to Cluster[{clusterName}] has failed - RetryCount[{retryCount}] Connection[{adoNetConnectionstring}], Retrying");
                 })
             .ExecuteAsync(async () =>
             {
                 var clusterClient = GetClient(clusterName, serviceId, adoNetConnectionstring, builderAction);
                 await clusterClient.Connect().ConfigureAwait(false);
                 return clusterClient;
             }).ConfigureAwait(false);
    }

    private static IClusterClient GetClient(string clusterName, string serviceId, string adoNetConnectionstring, Action<ClientBuilder>? builderAction)
    {
        var builder = new ClientBuilder();
        builderAction?.Invoke(builder);
        var clusterClient = builder.UseConsulClustering(gatewayOptions =>
        {
            gatewayOptions.Address =
            new Uri("http://localhost:8500");
            gatewayOptions.KvRootFolder = "test";
        })
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = $"{clusterName}";
                options.ServiceId = $"{serviceId}";
            })
            .AddSimpleMessageStreamProvider("SMS")
            .Build();
        return clusterClient;
    }
}
