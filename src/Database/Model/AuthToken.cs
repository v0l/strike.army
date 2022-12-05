namespace StrikeArmy.Database.Model;

public class AuthToken
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; init; }
    public User User { get; init; } = null!;

    public string AccessToken { get; init; } = null!;
    
    public string RefreshToken { get; init; }  = null!;
    
    public DateTime Expires { get; init; }
    
    public DateTime Created { get; init; }
}
