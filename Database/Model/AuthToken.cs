namespace StrikeArmy.Database.Model;

public class AuthToken
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; init; }
    public User User { get; init; }
    
    public string AccessToken { get; init; }
    
    public DateTime Expires { get; init; }
    
    public DateTime Created { get; init; }
}
