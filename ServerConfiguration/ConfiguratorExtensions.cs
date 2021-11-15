using Microsoft.Extensions.Configuration;

namespace ServerConfiguration;

public static class ConfiguratorExtensions
{
    public static IConfigurationBuilder GetConfigurationBuilder(this string[] args) =>
        new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("hosting.json", optional: true);
}
