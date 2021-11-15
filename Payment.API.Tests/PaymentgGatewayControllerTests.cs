using Backend.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Payment.API.Controllers;
using Payment.Contracts;
using System.Security.Claims;
using Xunit;
using static LanguageExt.Prelude;

namespace Payment.API.Tests;

public class PaymentgGatewayControllerTests
{
    [Fact]
    public async Task ProcessShouldWorkAsExpected()
    {
        var paymentGatewayBackendClient = new Mock<IPaymentGatewayBackendCllient>(MockBehavior.Strict);
        paymentGatewayBackendClient
            .Setup(x => x.Execute(It.IsAny<PaymentGatewayId>(), It.IsAny<PaymentGatewayCommands>(), It.IsAny<int>()))
            .ReturnsAsync(Success<Error, IEnumerable<PaymentGatewayEvents>>(new List<PaymentGatewayEvents>
            {
                    new PaymentRequestProcessed(ShopperId.Init(), PaymentRequestCard.Init() with { CVV = new(010) }, new(10), PaymentCurrency.Init(), new("PRId"))
            }));

        var merchantService = new Mock<IMerchantService>(MockBehavior.Strict);
        merchantService.Setup(x => x.GetMerchantIntegerId(It.IsAny<string>())).ReturnsAsync(1);

        var fackClaimPrinciple = new Mock<ClaimsPrincipal>();


        IEnumerable<Claim> claims = new List<Claim>() {
                new Claim("Merchant_Id", "1234567")
            }.AsEnumerable();
        fackClaimPrinciple.Setup(e => e.Claims).Returns(claims);


        var sut = new PaymentGatewayController(paymentGatewayBackendClient.Object, merchantService.Object);
        sut.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = fackClaimPrinciple.Object
        };
        await sut.Process(new ProcessPaymentRequestCommand(ShopperId.Init(), PaymentRequestCard.Init() with { CVV = new(010) }, new(10), PaymentCurrency.Init()));

        paymentGatewayBackendClient.VerifyAll();
        merchantService.VerifyAll();
        fackClaimPrinciple.VerifyAll();
    }

    [Fact]
    public async Task GetTransactionsShouldWorkAsExpected()
    {
        var paymentGatewayBackendClient = new Mock<IPaymentGatewayBackendCllient>(MockBehavior.Strict);

        var merchantService = new Mock<IMerchantService>(MockBehavior.Strict);
        merchantService.Setup(x => x.GetMerchantIntegerId(It.IsAny<string>())).ReturnsAsync(1);
        merchantService.Setup(x => x.GetAllTransactions(It.IsAny<int>())).ReturnsAsync(new { });
        var fackClaimPrinciple = new Mock<ClaimsPrincipal>();
        IEnumerable<Claim> claims = new List<Claim>() {
                new Claim("Merchant_Id", "1234567")
            }.AsEnumerable();
        fackClaimPrinciple.Setup(e => e.Claims).Returns(claims);


        var sut = new PaymentGatewayController(paymentGatewayBackendClient.Object, merchantService.Object);
        sut.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = fackClaimPrinciple.Object
        };
        var result = await sut.GetTransactions();

        var okResult = Assert.IsType<OkObjectResult>(result);

        paymentGatewayBackendClient.VerifyAll();
        merchantService.VerifyAll();
        fackClaimPrinciple.VerifyAll();
    }

    [Fact]
    public async Task GetTransactionShouldWorkAsExpected()
    {
        var paymentGatewayBackendClient = new Mock<IPaymentGatewayBackendCllient>(MockBehavior.Strict);

        var merchantService = new Mock<IMerchantService>(MockBehavior.Strict);
        merchantService.Setup(x => x.GetMerchantIntegerId(It.IsAny<string>())).ReturnsAsync(1);
        merchantService.Setup(x => x.GetTransaction(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new { });
        var fackClaimPrinciple = new Mock<ClaimsPrincipal>();
        IEnumerable<Claim> claims = new List<Claim>() {
                new Claim("Merchant_Id", "1234567")
            }.AsEnumerable();
        fackClaimPrinciple.Setup(e => e.Claims).Returns(claims);


        var sut = new PaymentGatewayController(paymentGatewayBackendClient.Object, merchantService.Object);
        sut.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = fackClaimPrinciple.Object
        };
        var result = await sut.GetTransaction("12");

        var okResult = Assert.IsType<OkObjectResult>(result);

        paymentGatewayBackendClient.VerifyAll();
        merchantService.VerifyAll();
        fackClaimPrinciple.VerifyAll();
    }
}
