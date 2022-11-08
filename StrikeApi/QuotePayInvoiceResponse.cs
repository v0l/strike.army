using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public abstract class PayInvoiceResponseBase
{
    [JsonProperty("conversionRate")]
    public ConversionRate ConversionRate { get; init; } = null!;

    [JsonProperty("lnInvoiceAmount")]
    public CurrencyAmount? InvoiceAmount { get; init; }

    [JsonProperty("amount")]
    public CurrencyAmount Amount { get; init; } = null!;

    [JsonProperty("lnNetworkFee")]
    public CurrencyAmount? NetworkFee { get; init; }

    [JsonProperty("totalAmount")]
    public CurrencyAmount TotalAmount { get; init; } = null!;

    [JsonProperty("reward")]
    public CurrencyAmount? Reward { get; init; }
}

public class QuotePayInvoiceResponse : PayInvoiceResponseBase
{
    [JsonProperty("paymentQuoteId")]
    public Guid PaymentQuoteId { get; init; }
    
    [JsonProperty("description")]
    public string? Description { get; init; }
    
    [JsonProperty("validUntil")]
    public DateTime ValidUntil { get; init; }
}

public class ExecutePayInvoiceResponse : PayInvoiceResponseBase
{
    [JsonProperty("result")]
    public string Result { get; init; } = null!;
    
    [JsonProperty("delivered")]
    public DateTime Delivered { get; init; }
}