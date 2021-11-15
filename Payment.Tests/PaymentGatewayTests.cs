using LanguageExt.Common;
using Payment.Contracts;
using System.Collections.Immutable;
using Xunit;

namespace Payment.Tests;

public class PaymentGatewayTests
{
    private static readonly ShopperId _validShopperId = ShopperId.Init();
    private static readonly ShopperId _invalidShopperId = new("");

    private static readonly MerchantId _validMerchantId = MerchantId.Init();
    private static readonly MerchantId _invalidMerchantId = new("");

    private static readonly PaymentRequestCard _paymentRequestCard = PaymentRequestCard.Init() with { CVV = new(010) };
    private static readonly PaymentAmount _paymentAmount = new(10);
    private static readonly PaymentCurrency _paymentCurrency = PaymentCurrency.Init();

    private static readonly ProcessPaymentRequestCommand _validPaymentRequestCommand = new(_validShopperId, _paymentRequestCard, _paymentAmount, _paymentCurrency);
    private static readonly PaymentRequestProcessed _paymentRequestInitialised;

    static PaymentGatewayTests()
    {
        var prIdValue = "paymentRequest";
        PaymentGatewayCommandHandlers.OneTimeNextId = prIdValue;
        _paymentRequestInitialised = new(_validShopperId, _paymentRequestCard, _paymentAmount, _paymentCurrency, new PaymentRequestId(prIdValue));
    }

    public static IEnumerable<object?[]> TestCases => new List<object?[]>
        {
            new object?[] { new PaymentGatewayState(0), _validPaymentRequestCommand, ImmutableArray<PaymentGatewayEvents>.Empty.Add(_paymentRequestInitialised), null },
            new object?[] { new PaymentGatewayState(0), _validPaymentRequestCommand with { ShopperId = _invalidShopperId }, ImmutableArray<PaymentGatewayEvents>.Empty, "Invalid ShopperId" }
        };

    [Theory]
    [MemberData(nameof(TestCases))]
    public void PaymentGatewayCommandHandlersShouldWorkAsExpected(PaymentGatewayState state,
                                                                  PaymentGatewayCommands command,
                                                                  ImmutableArray<PaymentGatewayEvents> events,
                                                                  Error error)
    {
        var sut = new PaymentGateway(new("PaymentGatewayId"));
        var result = sut.Execute(state, command);
        _ = result.Match(evts =>
        {
            var paymentGatewayEvents = evts.ToImmutableArray();
            Assert.True(events.All(x => paymentGatewayEvents.Contains(x)));
        }, Fail: errors =>
        {
            Assert.True(errors.Contains(error));
        });
    }

    [Fact]
    public void PaymentProcessedShouldIncrementNoOfPayments()
    {
        Assert.True(
            new PaymentGateway(new("PaymentGatewayId"))
            .ApplyEvent(new PaymentGatewayState(0),
                        new PaymentRequestProcessed(_validShopperId, _paymentRequestCard, _paymentAmount, _paymentCurrency, new PaymentRequestId("PrID")))
            .NumberOfPaymentsProcessed
        == 1);
    }
}
