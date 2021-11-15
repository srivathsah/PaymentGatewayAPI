using Domain;

namespace Payment.Contracts;

public abstract record PaymentRequestCommands : DomainCommand;
public record InitialisePaymentRequestCommand(ShopperId ShopperId,
                                              MerchantId MerchantId,
                                              PaymentRequestCard Card,
                                              PaymentAmount Amount,
                                              PaymentCurrency Currency) : PaymentRequestCommands;
public record AcceptPaymentRequestCommand() : PaymentRequestCommands;
public record RejectPaymentRequestCommand(PaymentRequestRejectedReason Reason) : PaymentRequestCommands;
