using StrikeArmy.StrikeApi;

namespace StrikeArmy;

public class StrikeArmyConfig
{
    public Uri? BaseUrl { get; init; }

    public StrikeApiSettings Strike { get; init; }

    public Guid Secret { get; init; } = Guid.NewGuid();
}