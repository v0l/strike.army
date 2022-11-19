using StrikeArmy.StrikeApi;

namespace StrikeArmy;

public class StrikeArmyConfig
{
    public Uri? BaseUrl { get; init; }

    public StrikeApiSettings Strike { get; init; }

    public Guid Secret { get; init; } = Guid.NewGuid();
    
    public JwtSettings JwtSettings { get; init; }
}

public class JwtSettings
{
    public string Issuer { get; init; } = null!;
    public string Secret { get; init; } = null!;
}
