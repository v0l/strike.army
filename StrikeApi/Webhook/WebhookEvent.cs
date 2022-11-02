using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

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
