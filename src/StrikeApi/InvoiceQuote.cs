using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public class InvoiceQuote
{
    [JsonProperty("quoteId")]
    public Guid QuoteId { get; init; }
    
    [JsonProperty("description")]
    public string? Description { get; init; }
    
    [JsonProperty("lnInvoice")]
    public string? LnInvoice { get; init; }
    
    [JsonProperty("onchainAddress")]
    public string? OnChainAddress { get; init; }
    
    [JsonProperty("expiration")]
    public DateTimeOffset Expiration { get; init; }
    
    [JsonProperty("expirationInSec")]
    public ulong ExpirationSec { get; init; }
    
    [JsonProperty("targetAmount")]
    public CurrencyAmount? TargetAmount { get; init; }
    
    [JsonProperty("sourceAmount")]
    public CurrencyAmount? SourceAmount { get; init; }
    
    [JsonProperty("conversionRate")]
    public ConversionRate? ConversionRate { get; init; }
}
