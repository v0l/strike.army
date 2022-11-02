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
