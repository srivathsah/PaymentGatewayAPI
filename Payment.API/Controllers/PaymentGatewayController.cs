using Backend.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Client;
using Payment.Contracts;

namespace Payment.API.Controllers;

public class PaymentGatewayController : DomainController<IPaymentGatewayClient, PaymentGatewayId, PaymentGatewayCommands, PaymentGatewayEvents>
{
    private readonly IMerchantService _merchantService;

    public PaymentGatewayController(IPaymentGatewayBackendCllient paymentGatewayBackendCllient, IMerchantService merchantService) : base(paymentGatewayBackendCllient, merchantService)
    {
        _merchantService = merchantService;
    }

    private async Task<IActionResult> Execute(PaymentGatewayCommands command) =>
        await ExecuteCommand(new PaymentGatewayId(GetMerchantId().ToString()), command, GetMerchantId());

    [Authorize]
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequestCommand processPaymentRequestCommand) =>
        await Execute(processPaymentRequestCommand);

    [Authorize]
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions() =>
        Ok(await _merchantService.GetAllTransactions(GetMerchantId()));

    [Authorize]
    [HttpGet("transactions/{id}")]
    public async Task<IActionResult> GetTransaction(string id) =>
        Ok(await _merchantService.GetTransaction(GetMerchantId(), id));
}
