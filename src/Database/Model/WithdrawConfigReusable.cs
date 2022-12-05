namespace StrikeArmy.Database.Model;

public class WithdrawConfigReusable
{
    public WithdrawConfigLimitInterval Interval { get; init; }
    
    public ulong Limit { get; init; }
}

public enum WithdrawConfigLimitInterval
{
    Daily,
    Weekly
}
