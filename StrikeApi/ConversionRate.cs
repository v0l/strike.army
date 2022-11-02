using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StrikeArmy.StrikeApi;

public class ConversionRate
{
    [JsonProperty("amount")]
    public string? Amount { get; init; }
    
    [JsonProperty("sourceCurrency")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Currencies Source { get; init; }
    
    [JsonProperty("targetCurrency")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Currencies Target { get; init; }
}
