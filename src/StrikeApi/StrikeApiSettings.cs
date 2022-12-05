namespace StrikeArmy.StrikeApi;

public class StrikeApiSettings
{
    public Uri Uri { get; init; } = new("https://api.strike.me");
    public string ApiKey { get; init; } = null!;
    
    public string ClientSecret { get; init; } = null!;
    
    public string ClientId { get; init; } = null!;

    public Uri AuthEndpoint { get; init; } = new("https://auth.strike.me");
}
