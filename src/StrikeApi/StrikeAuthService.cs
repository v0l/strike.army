using Microsoft.Extensions.Caching.Memory;
using StrikeArmy.Services;

namespace StrikeArmy.StrikeApi;

public class StrikeAuthService : OAuthService
{
    private readonly StrikeArmyConfig _config;

    public StrikeAuthService(HttpClient client, StrikeArmyConfig config, IMemoryCache cache)
        : base(client, cache)
    {
        _config = config;
    }

    protected override string[] Scopes => new[]
    {
        "offline_access",
        "partner.balances.read",
        "partner.payment-quote.lightning.create",
        "partner.payment-quote.execute"
    };

    protected override Uri BaseUri => _config.Strike.AuthEndpoint!;
    protected override Uri RedirectUri => new(_config.BaseUrl!, "/auth/token");
    protected override string ClientId => _config.Strike.ClientId;
    protected override string ClientSecret => _config.Strike.ClientSecret;
}
