namespace DataAccess;

public interface IConnectionStringFactory
{
    string GetConnectionString(string key);
}
