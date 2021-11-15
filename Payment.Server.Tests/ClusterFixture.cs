using Orleans.TestingHost;

namespace Payment.Server.Tests;

public class ClusterFixture : IDisposable
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloBuilderConfigurator>();
        this.Cluster = builder.Build();
        this.Cluster.Deploy();
    }

    public void Dispose()
    {
        this.Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; private set; }
}
