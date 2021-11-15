namespace Serialization;

public interface ISerializer
{
    string SerializeObject(object item);
    object? DeserializeObject(string json);
    T? DeserializeObject<T>(string json);
}
