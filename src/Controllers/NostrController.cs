using Microsoft.AspNetCore.Mvc;
using StrikeArmy.Services;

namespace StrikeArmy.Controllers;

public class NostrController
{
    private readonly StrikeArmyConfig _config;
    public NostrController(StrikeArmyConfig config)
    {
        _config = config;
    }

    [HttpGet("/.well-known/nostr.json")]
    public IActionResult NostrJson()
    {
        var pubkey = _config.Nostr?.GetHexPubKey();
        return new JsonResult(new
        {
            names = new
            {
                _ = pubkey
            }
        });
    }
}
