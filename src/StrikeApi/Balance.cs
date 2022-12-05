using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StrikeArmy.StrikeApi;

public class Balance
{
    [JsonProperty("total")]
    public decimal Total { get; init; }
    
    [JsonProperty("available")]
    public decimal Available { get; init; }

    [JsonProperty("outgoing")]
    public decimal Outgoing { get; init; }
    
    [JsonProperty("currency")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Currencies Currency { get; init; }
}
