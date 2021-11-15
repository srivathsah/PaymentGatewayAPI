namespace Payment.API;

public interface IMerchantService
{
    Task<int> GetMerchantIntegerId(string merchantId);
    Task<dynamic> GetAllTransactions(int merchantId);
    Task<dynamic> GetTransaction(int merchantId, string id);
}
