namespace StrikeArmy.Database.Model;

public class User
{
    public Guid Id { get; init; }
    public Guid StrikeUserId { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
    
    public List<AuthToken> AuthTokens { get; init; }
    public List<WithdrawConfig> WithdrawConfigs { get; init; }
}
