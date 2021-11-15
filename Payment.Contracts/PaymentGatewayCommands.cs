using Domain;

namespace Payment.Contracts;

public abstract record PaymentGatewayCommands : DomainCommand;
public record ProcessPaymentRequestCommand(ShopperId ShopperId,
                                           PaymentRequestCard Card,
                                           PaymentAmount Amount,
                                           PaymentCurrency Currency) : PaymentGatewayCommands;
