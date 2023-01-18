using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;


public abstract class WebhookBase
{
    [JsonProperty("webhookUrl")]
    public Uri? Uri { get; init; }
    
    [JsonProperty("webhookVersion")]
    public string? Version { get; init; }
    
    [JsonProperty("enabled")]
    public bool? Enabled { get; init; }

    [JsonProperty("eventTypes")]
    public HashSet<string>? EventTypes { get; init; }
}

public sealed class NewWebhook : WebhookBase
{
    [JsonProperty("secret")]
    public string? Secret { get; init; }
}

public sealed class WebhookSubscription : WebhookBase
{
    [JsonProperty("id")]
    public Guid? Id { get; init; }
    
    [JsonProperty("created")]
    public DateTimeOffset? Created { get; init; }
}

public class WebhookData
{
    [JsonProperty("entityId")]
    public Guid? EntityId { get; set; }

    [JsonProperty("changes")]
    public List<string>? Changes { get; set; }
}

public class WebhookEvent
{
    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("eventType")]
    public string? EventType { get; set; }

    [JsonProperty("webhookVersion")]
    public string? WebhookVersion { get; set; }

    [JsonProperty("data")]
    public WebhookData? Data { get; set; }

    [JsonProperty("created")]
    public DateTimeOffset? Created { get; set; }

    [JsonProperty("deliverySuccess")]
    public bool? DeliverySuccess { get; set; }

    public override string ToString()
    {
        return $"Id = {Id}, EntityId = {Data?.EntityId}, Event = {EventType}";
    }
}
