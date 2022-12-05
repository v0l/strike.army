using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public class CreateInvoiceRequest
{
    [JsonProperty("correlationId")]
    public string? CorrelationId { get; init; }
    
    [JsonProperty("description")]
    public string? Description { get; init; }
    
    [JsonProperty("amount")]
    public CurrencyAmount? Amount { get; init; }
    
    [JsonProperty("handle")]
    public string? Handle { get; init; }
}
