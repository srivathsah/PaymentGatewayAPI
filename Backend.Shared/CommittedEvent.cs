namespace Backend.Shared;

public class CommittedEvent
{
    public object? DomainEvent { get; set; }
    public MerchantId? MerchantId { get; set; }
    public string? SenderId { get; set; }
}
