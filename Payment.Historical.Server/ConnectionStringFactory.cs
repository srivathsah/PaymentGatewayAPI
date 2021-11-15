using DataAccess;

namespace Payment.Historical.Server;

public class ConnectionStringFactory : IConnectionStringFactory
{
    private readonly IConfiguration _configuration;

    public ConnectionStringFactory(IConfiguration configuration) => _configuration = configuration;

    public string GetConnectionString(string key) => _configuration.GetSection("Historical").GetValue<string>("ConnectionString");
}
