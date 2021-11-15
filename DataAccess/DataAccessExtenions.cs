using Dapper;
using DataAccess.Common;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace DataAccess;

public static class DataAccessExtenions
{
    private static readonly Dictionary<string, DbConnection> _cachedConnectionStrings = new();

    public static async Task<IEnumerable<T>> QueryAsync<T>(this string connectionString,
                                                           string query,
                                                           object? param = null,
                                                           CommandType commandType = CommandType.StoredProcedure,
                                                           Action<IDbConnection>? onConnectionOpen = null) =>
        await QueryAsync<T>(connectionString, async conn =>
            await conn.QueryAsync<T>(query, param, commandType: commandType, commandTimeout: 300), onConnectionOpen: onConnectionOpen);

    public static async Task<IEnumerable<T>> QueryAsync<T>(this string connectionString,
                                                           Func<DbConnection, Task<IEnumerable<T>>> func,
                                                           Action<IDbConnection>? onConnectionOpen = null,
                                                           Action<Exception>? onError = null)
    {
        var connection = GetConnection(connectionString);

        try
        {
            connection.Open();
            onConnectionOpen?.Invoke(connection);
            return await func.Invoke(connection);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return new List<T>();
        }
        finally
        {
            connection.Dispose();
        }
    }

    public static async Task<IEnumerable<T>> QueryAsync<T, T1>(this string connectionString,
                                                               string storedProcedure,
                                                               object? param = null,
                                                               CommandType commandType = CommandType.StoredProcedure,
                                                               Func<T, T1, T>? mapper = null,
                                                               Action<IDbConnection>? onConnectionOpen = null)
    {
        return await QueryAsync<T>(connectionString, async conn =>
        {
            return await conn.QueryAsync<T, T1, T>(storedProcedure, map: mapper, param: param, commandType: commandType, commandTimeout: 300);
        }, onConnectionOpen: onConnectionOpen);
    }

    public static async Task<int> ExecuteAsync(this string connectionString, string storedProcedure, object? param = null, CommandType commandType = CommandType.StoredProcedure, Action<Exception>? onError = null)
    {
        var connection = GetConnection(connectionString);
        try
        {
            connection.Open();
            return await connection.ExecuteAsync(storedProcedure, param, commandType: commandType, commandTimeout: 300);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return 0;
        }
        finally
        {
            connection.Dispose();
        }
    }

    public static async Task<PaginatedResult<T>> QueryPaginatedAsync<T>(this string connectionString, Func<DbConnection, Task<PaginatedResult<T>>> func, Action<Exception>? onError = null)
    {
        var connection = connectionString.GetConnection();
        try
        {
            connection.Open();
            return await func.Invoke(connection);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return new PaginatedResult<T>();
        }
        finally
        {
            connection.Dispose();
        }
    }

    public static async Task<PaginatedResult<T>> QueryPaginatedAsync<T>(this string connectionString,
                                                                        string storedProcedure,
                                                                        object? param = null,
                                                                        CommandType commandType = CommandType.StoredProcedure,
                                                                        Func<string, T?>? materializer = null)
    {
        return await QueryPaginatedAsync(connectionString, async conn =>
        {
            var result = new PaginatedResult<T>();
            List<T>? list = default;
            var result1 = await conn.QueryAsync(storedProcedure, (Func<dynamic, long, T?>)((row, count) =>
            {
                if (result.Data == null)
                {
                    list = new List<T>();
                    result.Data = list ?? Enumerable.Empty<T>();
                    result.DataLength = (int)count;
                }

                T? t = materializer?.Invoke(row?.jsondata);
                if (t != null)
                    list?.Add(t);
                return t ?? default;
            }), param, splitOn: "Count");
            return result;
        });
    }

    public static DbConnection GetConnection(this string connectionString) => new NpgsqlConnection(connectionString);
}
