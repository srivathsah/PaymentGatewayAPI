namespace DataAccess;

public interface IBaseDataAccess
{
    Task CreateSchema(string connectionString, string schemaName);
    Task<IEnumerable<T>> QueryAsync<T>(string connectionString, string sql, object param, Func<string, T?>? materializer = null);
    Task<int> ExecuteAsync(string connectionString, string sql, object param);
}
