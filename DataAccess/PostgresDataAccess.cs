using Npgsql;
using Serialization;
using System.Data;
using System.Data.Common;

namespace DataAccess;

public class PostgresDataAccess : IBaseDataAccess
{
    public PostgresDataAccess(ISerializer serializer) { }

    public async Task CreateSchema(string connectionString, string schemaName)
    {
        var connection = connectionString.GetConnection();
        DbTransaction? transaction = default;
        try
        {
            await connection.OpenAsync();
            transaction = connection.BeginTransaction();
            string cmdText = @$"CREATE SCHEMA IF NOT EXISTS {schemaName};";
            using var schemaCommand = new NpgsqlCommand(cmdText, connection as NpgsqlConnection, transaction as NpgsqlTransaction);
            await schemaCommand.ExecuteNonQueryAsync();
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction?.Rollback();
        }
        finally
        {
            connection.Dispose();
            transaction?.Dispose();
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string connectionString, string sql, object param, Func<string, T?>? materializer = null) =>
        await connectionString.QueryAsync<T>(sql, param, CommandType.Text);

    public async Task<int> ExecuteAsync(string connectionString, string sql, object param) =>
        await connectionString.ExecuteAsync(sql, param, CommandType.Text);

    public async Task CreateDB(string connectionString)
    {
        var cb = new NpgsqlConnectionStringBuilder(connectionString);
        var cs = $"Host={cb.Host};Database=postgres;User Id={cb.Username};Password={cb.Password};";
        var db = cb.Database;
        if (db != null)
            if (!await DbExists(cs, db))
            {
                try
                {
                    db = db.ToLower();
                    var exists = await DbExists(cs, db);
                    if (!exists)
                    {
                        using var conn = new NpgsqlConnection(cs);
                        await conn.OpenAsync();
                        using var command = new NpgsqlCommand($"CREATE DATABASE {db};", conn);
                        await command.ExecuteReaderAsync();
                        await conn.CloseAsync();
                    }
                }
                catch (Exception)
                {
                }
            }
    }

    public async Task<bool> DbExists(string csFull, string dbName)
    {
        using var connection = new NpgsqlConnection(csFull);
        dbName = dbName.ToLower();
        using var existsCommand = new NpgsqlCommand($"SELECT DATNAME FROM pg_catalog.pg_database WHERE DATNAME = '{dbName}'", connection);
        try
        {
            await connection.OpenAsync();
            var i = existsCommand.ExecuteScalar();
            await connection.CloseAsync();
            return i?.ToString()?.Equals(dbName) == true;
        }
        catch (Exception) { return false; }
    }
}
