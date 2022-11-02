using Newtonsoft.Json;

namespace StrikeArmy.StrikeApi;

public class Profile
{
    [JsonProperty("handle")]
    public string Handle { get; init; } = null;
    
    [JsonProperty("avatarUrl")]
    public string? AvatarUrl { get; init; }
    
    [JsonProperty("description")]
    public string? Description { get; init; }
    
    [JsonProperty("canReceive")]
    public bool CanReceive { get; init; }
    
    [JsonProperty("currencies")]
    public List<AvailableCurrency> Currencies { get; init; } = new();
}
