using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public sealed class WebhookSubscription : WebhookBase
{
    [JsonProperty("id")]
    public Guid? Id { get; init; }
    
    [JsonProperty("created")]
    public DateTimeOffset? Created { get; init; }
}
