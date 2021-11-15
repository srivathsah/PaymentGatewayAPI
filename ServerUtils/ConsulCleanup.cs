using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Runtime;

namespace ServerUtils;

class ConsulCleanup : IHostedService
{
    private readonly ILocalSiloDetails _localSiloDetails;
    private readonly IMembershipTable _membershipTable;

    public ConsulCleanup(ILocalSiloDetails localSiloDetails, IMembershipTable membershipTable)
    {
        _localSiloDetails = localSiloDetails;
        _membershipTable = membershipTable;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this._membershipTable.UpdateIAmAlive(new MembershipEntry
        {
            SiloAddress = this._localSiloDetails.SiloAddress,
            IAmAliveTime = DateTime.UtcNow.AddMinutes(-10),
            Status = SiloStatus.Dead, // This isn't used to determine if a silo is defunct but I'm going to set it anyway.
        });

        await this._membershipTable.CleanupDefunctSiloEntries(DateTimeOffset.UtcNow.AddMinutes(-5));
    }
}
