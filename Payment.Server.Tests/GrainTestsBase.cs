using Domain;
using Domain.EventSourcing;
using Moq;
using Orleans.Concurrency;
using Orleans.TestingHost;
using Xunit;

namespace Payment.Server.Tests;

public abstract class GrainTestsBase<T, TId, TState, TCommand, TEvent>
    where T : IDomainClient<TId, TCommand, TEvent>
    where TCommand : DomainCommand
    where TEvent : DomainEvent
    where TId : ValueRecord<string>
{
    private readonly TestCluster _cluster;

    protected Func<TCommand>? ValidCommandFactory { get; set; }
    protected Func<TCommand>? InvalidCommandFactory { get; set; }

    public GrainTestsBase(ClusterFixture fixture) => _cluster = fixture.Cluster;

    [Fact]
    public async Task ExecuteShouldSucceedOnSuccessDomainResult()
    {
        var client = _cluster.GrainFactory.GetGrain<T>("grainId");
        TestSiloBuilderConfigurator.SetupSnapshotDataAccess<TState>();
        TestSiloBuilderConfigurator.MockEventsDataAccess.Setup(x => x.SaveEvent(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), -1)).Returns(Task.CompletedTask);
        if (ValidCommandFactory != null)
        {
            var result = await client.Execute(new Immutable<TCommand>(ValidCommandFactory()), -1).ConfigureAwait(false);
            Assert.True(result.Value.Match(val => true, err => false));
            TestSiloBuilderConfigurator.MockEventsDataAccess.VerifyAll();
        }
        else
        {
            throw new Exception("CommandFactory is null");
        }
    }

    [Fact]
    public async Task ExecuteShouldFailOnFailureDomainResult()
    {
        var client = _cluster.GrainFactory.GetGrain<T>("grainId");
        TestSiloBuilderConfigurator.SetupSnapshotDataAccess<TState>();
        if (InvalidCommandFactory != null)
        {
            var result = await client.Execute(new Immutable<TCommand>(InvalidCommandFactory()), -1).ConfigureAwait(false);
            Assert.True(result.Value.Match(val => false, err => true));
        }
        else
        {
            throw new Exception("CommandFactory is null");
        }
    }
}
