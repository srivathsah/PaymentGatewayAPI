namespace Payment.API;

public class MerchantService : IMerchantService
{
    private readonly HttpClient _httpClient;

    public MerchantService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Historical");
    }

    public async Task<dynamic> GetAllTransactions(int merchantId)
    {
        var result = await _httpClient.GetAsync($"api/historical/transactions/{merchantId}");
        return await result.Content.ReadAsStringAsync();
    }

    public Task<int> GetMerchantIntegerId(string merchantId) => Task.FromResult(merchantId == "0oa2k4h6bd86aevzf5d7" ? 1 : 2);

    public async Task<dynamic> GetTransaction(int merchantId, string id)
    {
        var result = await _httpClient.GetAsync($"api/historical/transactions/{merchantId}/{id}");
        return await result.Content.ReadAsStringAsync();
    }
}
