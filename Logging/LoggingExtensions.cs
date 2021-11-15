using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.Loki;

namespace Logging
{
    public static class LoggingExtensions
    {
        public static void AddLokiLogger(this ILoggingBuilder loggingBuilder, string lokiServer, string? username = null, string? password = null)
        {
            Log.Logger = GetLoggerConfiguration()
                .Async(a => a
                    .LokiHttp(() => new LokiSinkConfiguration { LokiUrl = lokiServer }))
                    .CreateLogger();
            loggingBuilder.AddSerilog(dispose: true);
        }

        public static void AddFileLogger(this ILoggingBuilder loggingBuilder, string filename)
        {
            Log.Logger = GetLoggerConfiguration()
                .Async(a => a
                .File(filename, shared: true, rollingInterval: RollingInterval.Day, outputTemplate:
                                    "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} {ProcessName:D5} {ThreadId:D3} {Log4NetLevel}  {Message:lj}{NewLine}{Exception}"))
                .CreateLogger();
            loggingBuilder.AddSerilog();
        }

        private static LoggerSinkConfiguration GetLoggerConfiguration() =>
            new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadName()
                    .Enrich.WithProcessName()
                    .WriteTo;
    }
}
