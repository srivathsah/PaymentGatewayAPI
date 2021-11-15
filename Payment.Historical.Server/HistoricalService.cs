using DataAccess;
using Payment.Contracts;

namespace Payment.Historical.Server;

public class HistoricalService : IHistoricalService
{
    private readonly IBaseDataAccess _baseDataAccess;
    private readonly string _connectionString;

    public HistoricalService(IBaseDataAccess baseDataAccess, IConnectionStringFactory connectionStringFactory)
    {
        _baseDataAccess = baseDataAccess;
        _connectionString = connectionStringFactory.GetConnectionString("");
    }

    public async Task<IEnumerable<dynamic>> GetAllTransactions(int merchantId)
    {
        return await _baseDataAccess.QueryAsync<dynamic>(_connectionString, "SELECT * FROM payments.transactions WHERE merchant_id = @merchantId", new { merchantId });
    }

    public async Task<dynamic> GetTransaction(int merchantId, string id)
    {
        var result = await _baseDataAccess.QueryAsync<dynamic>(_connectionString, "SELECT * FROM payments.transactions WHERE merchant_id = @merchantId AND payment_request_token = @id",
            new { merchantId, id });
        return result.FirstOrDefault() ?? new { };
    }

    public async Task OnPaymentRequestAccepted(PaymentRequestAccepted paymentRequestAccepted, string id)
    {
        await _baseDataAccess.ExecuteAsync(_connectionString, $@"
UPDATE
    payments.transactions
SET
    status = @status,
    update_time = @updateTime
WHERE
    payment_request_token = @paymentRequestToken", new
        {
            paymentRequestToken = id,
            status = "Accepted",
            updateTime = DateTimeOffset.UtcNow
        });
    }

    public async Task OnPaymentRequestInitialised(PaymentRequestInitialised message, string id)
    {
        await _baseDataAccess.ExecuteAsync(_connectionString, $@"
INSERT INTO payments.transactions(payment_request_token, merchant_id, shopper_id, card_number, card_cvv, card_expiry_month, card_expiry_year, amount, payment_currency, status, update_time)
VALUES(@paymentRequestToken, @merchantId, @shopperId, @cardNumber, @cardCvv, @cardExpiryMonth, @cardExpiryYear, @amount, @currency, @status, @updateTime)
", new
        {
            paymentRequestToken = id,
            merchantId = int.Parse(message.MerchantId.Value),
            shopperId = message.ShopperId.Value,
            cardNumber = new string('X', message.Card.Number.Value.Length - 4) + message.Card.Number.Value[^4..],
            cardCvv = message.Card.CVV.Value,
            cardExpiryMonth = message.Card.Expiry.Month.Value,
            cardExpiryYear = message.Card.Expiry.Year.Value,
            amount = message.Amount.Value,
            currency = message.Currency.Value,
            status = "Started",
            updateTime = DateTimeOffset.UtcNow
        });
    }

    public async Task OnPaymentRequestRejected(PaymentRequestRejected paymentRequestRejected, string id)
    {
        await _baseDataAccess.ExecuteAsync(_connectionString, $@"
UPDATE
    payments.transactions
SET
    status = @status,
    update_time = @updateTime
WHERE
    payment_request_token = @paymentRequestToken", new
        {
            paymentRequestToken = id,
            status = "Rejected",
            updateTime = DateTimeOffset.UtcNow
        });
    }
}
