using Domain;

namespace Payment.Contracts;

public abstract record PaymentGatewayEvents : DomainEvent;
public record PaymentRequestProcessed(ShopperId ShopperId,
                                      PaymentRequestCard Card,
                                      PaymentAmount Amount,
                                      PaymentCurrency Currency, PaymentRequestId Id) : PaymentGatewayEvents;
