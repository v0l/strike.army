using StrikeArmy.Services;

namespace StrikeArmy.Database.Model;

public class WithdrawConfig
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }
    public User User { get; init; } = null!;

    public ulong? Min { get; init; }

    public ulong? Max { get; init; }

    public WithdrawConfigType Type { get; init; }

    public string Description { get; init; } = null!;

    public WithdrawConfigReusable? ConfigReusable { get; init; }

    public List<WithdrawConfigPayment> Payments { get; init; } = new();
    
    public BoltCardConfig? BoltCardConfig { get; init; }

    public ulong? Remaining => this.GetRemainingUsage();
}

public enum WithdrawConfigType
{
    SingleUse,
    Reusable
}
