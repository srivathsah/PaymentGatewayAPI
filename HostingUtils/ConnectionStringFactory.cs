using DataAccess;
using Microsoft.Extensions.Configuration;

namespace HostingUtils;

class ConnectionStringFactory : IConnectionStringFactory
{
    private readonly IConfiguration _configuration;
    public ConnectionStringFactory(IConfiguration configuration) => _configuration = configuration;
    public string GetConnectionString(string key) => _configuration.GetSection(key).GetValue<string>("ConnectionString");
}
