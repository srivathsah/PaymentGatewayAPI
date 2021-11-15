using DataAccess;
using Serialization;

namespace Payment.Historical.Server;

public static class DataAccessExtensions
{
    private const string SchemaName = "payments";
    private const string TransactionsTableName = "transactions";

    public static async Task CreateSchema(this string connectionString)
    {
        PostgresDataAccess dataAccess = new(new Serializer());
        await dataAccess.CreateDB(connectionString);
        await dataAccess.CreateSchema(connectionString, SchemaName);
        var createTableText = @$"
CREATE TABLE IF NOT EXISTS {SchemaName}.{TransactionsTableName}
(
    id SERIAL PRIMARY KEY NOT NULL,
    payment_request_token TEXT NOT NULL,
    merchant_id INT NOT NULL,
    shopper_id TEXT NOT NULL,
    card_number TEXT NOT NULL,
    card_cvv INT NOT NULL,
    card_expiry_month INT NOT NULL,
    card_expiry_year INT NOT NULL,
    amount DECIMAL NOT NULL,
    payment_currency TEXT NOT NULL,
    status TEXT NOT NULL,
    update_time timestamptz
);";

        await dataAccess.ExecuteAsync(connectionString, createTableText, null);
    }
}
