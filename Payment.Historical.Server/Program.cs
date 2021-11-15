using Npgsql;
using Payment.Historical.Server;

NpgsqlConnection.GlobalTypeMapper.UseNodaTime();
await new HistoricalStartup().RunHostAsync(new CancellationToken());
