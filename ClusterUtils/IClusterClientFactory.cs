using Orleans;

namespace ClusterUtils;

public interface IClusterClientFactory
{
    Task<IClusterClient> GetClusterClient(int clusterPort, string cluster, string service, Action<ClientBuilder>? builderAction = null);
}
