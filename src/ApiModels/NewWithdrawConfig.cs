using StrikeArmy.Database.Model;

namespace StrikeArmy.ApiModels;

public class NewWithdrawConfig
{
    public WithdrawConfigType Type { get; init; }
    public string Description { get; init; }
    public ulong Min { get; init; }
    public ulong Max { get; init; }
    public WithdrawConfigLimitInterval? Interval { get; init; }
    public ulong? Limit { get; init; }
    public bool BoltCard { get; init; }
}
