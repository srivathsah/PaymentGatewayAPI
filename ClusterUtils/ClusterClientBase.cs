using Domain;
using Domain.EventSourcing;
using LanguageExt;
using LanguageExt.Common;
using Orleans;
using Orleans.Concurrency;

namespace ClusterUtils;

public abstract class ClusterClientBase<T, TId, TCommand, TEvent> : IClusterClientBase
    where T : IDomainClient<TId, TCommand, TEvent>
    where TCommand : DomainCommand
    where TEvent : DomainEvent
    where TId : ValueRecord<string>
{
    private readonly int _clusterPort;
    private readonly string _cluster;
    private readonly string _service;
    private readonly IClusterClientFactory _clusterClientFactory;
    private readonly Action<ClientBuilder>? _builderAction;

    private readonly string _cc = Guid.NewGuid().ToString();

    public IClusterClient? ClusterClient { get; private set; }

    protected ClusterClientBase(
        int clusterPort, string cluster, string service,
        IClusterClientFactory clusterClientFactory, Action<ClientBuilder>? builderAction = null)
    {
        _clusterPort = clusterPort;
        _cluster = cluster;
        _service = service;
        _clusterClientFactory = clusterClientFactory;
        _builderAction = builderAction;
    }

    public async Task TryInit() =>
        ClusterClient = await _clusterClientFactory.GetClusterClient(_clusterPort, _cluster, _service, _builderAction);

    public virtual async Task<Validation<Error, IEnumerable<TEvent>>> Execute(TId id, TCommand command, int MerchantId) =>
        (await id.ValidatedValue.Match<Task<Immutable<Validation<Error, IEnumerable<TEvent>>>>>(
            async val =>
            {
                try
                {
                    if (ClusterClient == null) return new Immutable<Validation<Error, IEnumerable<TEvent>>>(Error.New("ClientIsNull"));
                    return await ClusterClient.GetGrain<T>(val).Execute(new Immutable<TCommand>(command), MerchantId);
                }
                catch (Exception ex)
                {
                    await TryInit();
                    if (ClusterClient == null) return new Immutable<Validation<Error, IEnumerable<TEvent>>>(Error.New("ClientIsNull"));
                    return await ((T)ClusterClient.GetGrain<T>(val)).Execute(new Immutable<TCommand>(command), MerchantId);
                }
            },
            err => Task.FromResult(new Immutable<Validation<Error, IEnumerable<TEvent>>>(err))
        ).ConfigureAwait(false)).Value;
}
