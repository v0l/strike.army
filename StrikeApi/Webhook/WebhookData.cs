using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public class WebhookData
{
    [JsonProperty("entityId")]
    public Guid? EntityId { get; set; }

    [JsonProperty("changes")]
    public List<string>? Changes { get; set; }
}
