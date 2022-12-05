using Newtonsoft.Json;

namespace StrikeArmy.ApiModels;

public class BoltCardSetup
{
    [JsonProperty("protocol_name")]
    public string ProtocolName { get; init; } = "create_bolt_card_response";

    [JsonProperty("protocol_version")]
    public int ProtocolVersion { get; init; } = 1;

    [JsonProperty("card_name")]
    public string CardName { get; init; } = null!;

    [JsonProperty("lnurlw_base")]
    public Uri BaseUrl { get; init; } = null!;

    [JsonProperty("k0")]
    public string K0 { get; init; } = null!;

    [JsonProperty("k1")]
    public string K1 { get; init; } = null!;

    [JsonProperty("k2")]
    public string K2 { get; init; } = null!;

    [JsonProperty("k3")]
    public string K3 { get; init; } = null!;

    [JsonProperty("k4")]
    public string K4 { get; init; } = null!;
}
