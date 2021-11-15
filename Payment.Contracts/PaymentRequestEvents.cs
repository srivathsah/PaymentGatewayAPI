using Domain;

namespace Payment.Contracts;

public abstract record PaymentRequestEvents : DomainEvent;
public record PaymentRequestInitialised(ShopperId ShopperId,
                                        MerchantId MerchantId,
                                        PaymentRequestCard Card,
                                        PaymentAmount Amount,
                                        PaymentCurrency Currency) : PaymentRequestEvents;
public record PaymentRequestAccepted() : PaymentRequestEvents;
public record PaymentRequestRejected(PaymentRequestRejectedReason Reason) : PaymentRequestEvents;
