namespace StrikeArmy.Database.Model;

public class WithdrawConfigPayment
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid WithdrawConfigId { get; init; }

    public Guid StrikeQuoteId { get; init; }

    public DateTime Created { get; init; } = DateTime.UtcNow;

    public ulong Amount { get; init; }

    public ulong? RoutingFee { get; set; }

    public string PayeeNodePubKey { get; init; } = null!;

    public string Pr { get; init; } = null!;

    public PaymentStatus Status { get; set; }

    public string? StatusMessage { get; set; }
}

public enum PaymentStatus
{
    New,
    Paid,
    Pending,
    Failed
}
