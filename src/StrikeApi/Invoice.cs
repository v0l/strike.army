using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StrikeArmy.StrikeApi;

public class Invoice
{
    [JsonProperty("invoiceId")]
    public Guid InvoiceId { get; init; }
    
    [JsonProperty("amount")]
    public CurrencyAmount? Amount { get; init; }

    [JsonProperty("state")]
    [JsonConverter(typeof(StringEnumConverter))]
    public InvoiceState State { get; set; }

    [JsonProperty("created")]
    public DateTimeOffset? Created { get; init; }
    
    [JsonProperty("correlationId")]
    public string? CorrelationId { get; init; }
    
    [JsonProperty("description")]
    public string? Description { get; init; }
    
    [JsonProperty("issuerId")]
    public Guid? IssuerId { get; init; }
    
    [JsonProperty("receiverId")]
    public Guid? ReceiverId { get; init; }
    
    [JsonProperty("payerId")]
    public Guid? PayerId { get; init; }
}
