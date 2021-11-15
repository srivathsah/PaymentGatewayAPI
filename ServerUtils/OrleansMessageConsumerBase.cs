using Orleans;

namespace ServerUtils;

public abstract class OrleansMessageConsumerBase<T> : MessageConsumerBase<T> where T : class
{
    public OrleansMessageConsumerBase(IClusterClient clusterClient) => ClusterClient = clusterClient;

    protected IClusterClient ClusterClient { get; }
}
