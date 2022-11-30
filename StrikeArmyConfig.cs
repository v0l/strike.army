using StrikeArmy.StrikeApi;

namespace StrikeArmy;

public class StrikeArmyConfig
{
    public Uri? BaseUrl { get; init; }

    public StrikeApiSettings Strike { get; init; } = null!;

    public PlausibleSettings? Plausible { get; init; }
}

public sealed class PlausibleSettings
{
    public Uri Endpoint { get; init; } = null!;
    public string Domain { get; init; } = null!;
}