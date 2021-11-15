using MassTransit;
using Newtonsoft.Json;

namespace DistributedQueue;

class TypeNameHandlingConverter : JsonConverter
{
    private readonly TypeNameHandling _typeNameHandling;

    public TypeNameHandlingConverter(TypeNameHandling typeNameHandling) => _typeNameHandling = typeNameHandling;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        // Set TypeNameHandling for serializing objects with $type
        new JsonSerializer { TypeNameHandling = _typeNameHandling }.Serialize(writer, value);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
        // Set TypeNameHandling for deserializing objects with $type
        new JsonSerializer { TypeNameHandling = _typeNameHandling }.Deserialize(reader, objectType);

    public override bool CanConvert(Type objectType) => !IsMassTransitOrSystemType(objectType);

    private static bool IsMassTransitOrSystemType(Type objectType) =>
        objectType.Assembly == typeof(IConsumer).Assembly ||
               objectType.Assembly.IsDynamic ||
               objectType.Assembly == typeof(object).Assembly;
}
