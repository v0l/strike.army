namespace StrikeArmy.Database.Model;

public class WithdrawConfig
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; init; }
    public User User { get; init; }
    
    public ulong? Min { get; init; }
    
    public ulong? Max { get; init; }
}
