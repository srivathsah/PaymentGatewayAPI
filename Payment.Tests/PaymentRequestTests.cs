using LanguageExt.Common;
using Payment.Contracts;
using System.Collections.Immutable;
using Xunit;

namespace Payment.Tests;

public class PaymentRequestTests
{
    private static readonly PaymentRequestState _initialState = PaymentRequestState.Init();
    private static readonly ShopperId _validShopperId = ShopperId.Init();
    private static readonly ShopperId _invalidShopperId = new("");
    private static readonly MerchantId _validMerchantId = MerchantId.Init();

    private static readonly PaymentRequestCard _paymentRequestCard = PaymentRequestCard.Init() with { CVV = new CardCVV(010) };
    private static readonly PaymentAmount _paymentAmount = new(10);
    private static readonly PaymentCurrency _paymentCurrency = PaymentCurrency.Init();
    private static readonly InitialisePaymentRequestCommand _initialisePaymentRequestCommand = new(_validShopperId, _validMerchantId, _paymentRequestCard, _paymentAmount, _paymentCurrency);
    private static readonly PaymentRequestInitialised _paymentRequestInitialised = new(_validShopperId, _validMerchantId, _paymentRequestCard, _paymentAmount, _paymentCurrency);
    private static readonly PaymentRequestRejected _paymentRequestRejected = new(new("Error"));

    private static readonly PaymentRequestState _initialisedState = _initialState with { Amount = _paymentAmount, Currency = _paymentCurrency, Card = _paymentRequestCard, MerchantId = _validMerchantId, ShopperId = _validShopperId, Status = new ProcessStarted() };
    public static IEnumerable<object?[]> CommandHandlerTestCases => new List<object?[]>
        {
            new object?[] { _initialState       , _initialisePaymentRequestCommand                                         , ImmutableArray<PaymentRequestEvents>.Empty.Add(_paymentRequestInitialised)     , null  },
            new object?[] { _initialState       , _initialisePaymentRequestCommand with { ShopperId = _invalidShopperId }  , ImmutableArray<PaymentRequestEvents>.Empty                                     , "Invalid ShopperId"  },
            new object?[] { _initialisedState   , _initialisePaymentRequestCommand                                         , ImmutableArray<PaymentRequestEvents>.Empty                                     , "Invalid Status" },
            new object?[] { _initialisedState   , new AcceptPaymentRequestCommand()                                        , ImmutableArray<PaymentRequestEvents>.Empty.Add(new PaymentRequestAccepted())   , null },
            new object?[] { _initialisedState   , new RejectPaymentRequestCommand(new("Error"))                            , ImmutableArray<PaymentRequestEvents>.Empty.Add(_paymentRequestRejected)        , null }
        };

    [Theory]
    [MemberData(nameof(CommandHandlerTestCases))]
    public void PaymentRequestCommandHandlersShouldWorkAsExpected(PaymentRequestState state,
                                                                  PaymentRequestCommands command,
                                                                  ImmutableArray<PaymentRequestEvents> events,
                                                                  Error error)
    {
        var sut = new PaymentRequest(new("PaymentRequestId"));
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

    public static IEnumerable<object?[]> EventHandlerTestCases => new List<object?[]>
        {
            new object?[] { _initialState, _paymentRequestInitialised,  _initialisedState },
            new object?[] { _initialisedState, new PaymentRequestAccepted(),  _initialisedState with { Status = new AcceptedByBank() } },
            new object?[] { _initialisedState, new PaymentRequestRejected(new("Error")),  _initialisedState with { Status = new RejectedByBank(new("Error")) } },
        };

    [Theory]
    [MemberData(nameof(EventHandlerTestCases))]
    public void PaymentRequestEventHandlersShouldWorkAsExpected(PaymentRequestState startingState,
                                                                PaymentRequestEvents evt,
                                                                PaymentRequestState resultState)
    {
        Assert.True(new PaymentRequest(new("PaymentRequestId")).ApplyEvent(startingState, evt) == resultState);
    }
}
