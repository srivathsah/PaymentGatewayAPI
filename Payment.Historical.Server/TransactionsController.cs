using Microsoft.AspNetCore.Mvc;

namespace Payment.Historical.Server;

[Route("api/historical/[controller]")]
[Route("[controller]")]
[ApiController]
public class TransactionsController : Controller
{
    private readonly IHistoricalService _historicalService;

    public TransactionsController(IHistoricalService historicalService)
    {
        _historicalService = historicalService;
    }

    [HttpGet("{merchantId}")]
    public async Task<IActionResult> Get([FromRoute] int merchantId)
    {
        return Ok(await _historicalService.GetAllTransactions(merchantId));
    }

    [HttpGet("{merchantId}/{id}")]
    public async Task<IActionResult> Get([FromRoute] int merchantId, [FromRoute] string id)
    {
        return Ok(await _historicalService.GetTransaction(merchantId, id));
    }
}
