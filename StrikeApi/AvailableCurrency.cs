using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public class AvailableCurrency
{
    [JsonProperty("currency")]
    public Currencies Currency { get; init; }
    
    [JsonProperty("isDefaultCurrency")]
    public bool IsDefault { get; init; }
    
    [JsonProperty("isAvailable")]
    public bool IsAvailable { get; init; }
}
