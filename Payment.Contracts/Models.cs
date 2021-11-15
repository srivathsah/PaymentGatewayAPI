using Domain;
using static DateUtils.ZonedDateTimeUtils;

namespace Payment.Contracts;

public record PaymentGatewayId(string Value) : ValueRecord<string>(Value)
{
    public override bool Validate(string value) => !string.IsNullOrWhiteSpace(value);
}

public record ShopperId(string Value) : ValueRecord<string>(Value)
{
    public override bool Validate(string value) => !string.IsNullOrWhiteSpace(value);

    public static ShopperId Init() => new("ShopperId");
}

public record MerchantId(string Value) : ValueRecord<string>(Value)
{
    public override bool Validate(string value) => !string.IsNullOrWhiteSpace(value);
    public static MerchantId Init() => new("MerchantId");
}

public record PaymentRequestId(string Value) : ValueRecord<string>(Value)
{
    public override bool Validate(string value) => !string.IsNullOrWhiteSpace(value);
}

public record CardNumber(string Value) : ValueRecord<string>(Value)
{
    public override bool Validate(string value) => value?.Length == 16;
    public static CardNumber Init() => new("XXXXXXXXXXXXXXXX");
}

public record CardExpiryMonth(int Value) : ValueRecord<int>(Value)
{
    public override bool Validate(int value) => Value > 0 && Value < 13;
    public static CardExpiryMonth Init() => new(1);
}

public record CardExpiryYear(int Value) : ValueRecord<int>(Value)
{
    public override bool Validate(int value) => GetZonedDateTime().Year <= Value;
    public static CardExpiryYear Init() => new(2030);
}

public record CardCVV(int Value) : ValueRecord<int>(Value)
{
    public override bool Validate(int value) => value > 0 && value < 1000;
    public static CardCVV Init() => new(000);
}

public record PaymentAmount(decimal Value) : ValueRecord<decimal>(Value)
{
    public override bool Validate(decimal value) => value > 0;
    public static PaymentAmount Init() => new(0);
}

public record PaymentCurrency(string Value) : ValueRecord<string>(Value)
{
    public override bool Validate(string value) => value == "GBP";
    public static PaymentCurrency Init() => new("GBP");
}

public record PaymentRequestRejectedReason(string Value) : ValueRecord<string>(Value)
{
    public override bool Validate(string value) => !string.IsNullOrWhiteSpace(value);
}

public abstract record PaymentRequestStatus;
public record RequestStarted : PaymentRequestStatus;
public record ProcessStarted : PaymentRequestStatus;
public record AcceptedByBank : PaymentRequestStatus;
public record RejectedByBank(PaymentRequestRejectedReason Reason) : PaymentRequestStatus;

public record CardExpiry(CardExpiryMonth Month, CardExpiryYear Year)
{
    public static CardExpiry Init() => new(CardExpiryMonth.Init(), CardExpiryYear.Init());
}

public record PaymentRequestCard(CardNumber Number, CardExpiry Expiry, CardCVV CVV)
{
    public static PaymentRequestCard Init() => new(CardNumber.Init(), CardExpiry.Init(), CardCVV.Init());
}
