namespace StrikeArmy.StrikeApi;

public class StrikeApiSettings
{
    public Uri? Uri { get; init; }
    public string? ApiKey { get; init; }
    
    public string ClientSecret { get; init; }
    
    public string ClientId { get; init; }

    public Uri AuthEndpoint { get; init; } = new("https://auth.strike.me");
}
