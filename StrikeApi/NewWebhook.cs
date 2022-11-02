using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public sealed class NewWebhook : WebhookBase
{
    [JsonProperty("secret")]
    public string? Secret { get; init; }
}
