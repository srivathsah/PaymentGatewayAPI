using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using System.Reflection;

namespace DistributedQueue;

public static class DistributedQueueExtensions
{
    public static void AddDistributedQueue(this IServiceCollection services, string host, string queueName)
    {
        var consumers = GetMassTransitConsumers();
        consumers.ForEach(consumer => services.AddScoped(consumer));
        services.AddMassTransit(mt =>
        {
            foreach (var type in consumers)
            {
                mt.AddConsumer(type);
            }

            mt.UsingRabbitMq((context, configurator) =>
            {
                configurator.ConfigureJsonSerializer(settings =>
                {
                    settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.All));
                    return settings;
                });

                configurator.ConfigureJsonDeserializer(settings =>
                {
                    settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                    settings.Converters.Add(new TypeNameHandlingConverter(TypeNameHandling.All));
                    return settings;
                });

                configurator.Host(host);

                configurator.ReceiveEndpoint(queueName, ep =>
                {
                    foreach (var type in consumers)
                    {
                        ep.ConfigureConsumer(context, type);
                    }
                });
            });
        });

        static List<Type> GetMassTransitConsumers()
        {
            var assembly = Assembly.GetEntryAssembly();

            var consumerTypes = GetAllTypesImplementingOpenGenericType(typeof(IConsumer<>), assembly).Where(x => !x.IsAbstract).ToList();
            return consumerTypes;

            static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, Assembly assembly) => from x in assembly.GetTypes()
                                                                                                                        from z in x.GetInterfaces()
                                                                                                                        let y = x.BaseType
                                                                                                                        where
                                                                                                                        (y != null && y.IsGenericType &&
                                                                                                                        openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                                                                                                                        (z.IsGenericType &&
                                                                                                                        openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
                                                                                                                        select x;
        }
    }
}
