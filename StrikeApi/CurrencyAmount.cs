using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StrikeArmy.StrikeApi;

public class CurrencyAmount
{
    [JsonProperty("amount")]
    public string? Amount { get; init; }

    [JsonProperty("currency")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Currencies? Currency { get; init; }
}
