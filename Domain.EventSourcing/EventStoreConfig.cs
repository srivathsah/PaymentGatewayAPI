namespace Domain.EventSourcing;

public class EventStoreConfig
{

}

public class ConnectionString
{
    public string Server { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int Port { get; set; }
    public string Database { get; set; } = "template1";
}
