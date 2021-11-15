using Payment.Contracts;

namespace Payment.Historical.Server;

public interface IHistoricalService
{
    Task OnPaymentRequestInitialised(PaymentRequestInitialised paymentRequestInitialised, string id);
    Task OnPaymentRequestAccepted(PaymentRequestAccepted paymentRequestAccepted, string id);
    Task OnPaymentRequestRejected(PaymentRequestRejected paymentRequestRejected, string id);
    Task<IEnumerable<dynamic>> GetAllTransactions(int merchantId);
    Task<dynamic> GetTransaction(int merchantId, string id);
}
