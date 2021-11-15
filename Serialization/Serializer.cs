using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Serialization;

public class Serializer : ISerializer
{
    public Serializer()
    {
        JsonSerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    }

    public JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.All,
        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
    };

    public object? DeserializeObject(string json) => JsonConvert.DeserializeObject(json, JsonSerializerSettings);

    public T? DeserializeObject<T>(string json) => JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);

    public string SerializeObject(object item) => JsonConvert.SerializeObject(item, typeof(object), JsonSerializerSettings);
}
