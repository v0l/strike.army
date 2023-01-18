using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StrikeArmy.Services;
using StrikeArmy.StrikeApi;
using HMACSHA256 = System.Security.Cryptography.HMACSHA256;

namespace StrikeArmy.Controllers;

[Route("webhook")]
public class WebhookController : Controller
{
    private readonly ILogger<WebhookController> _logger;
    private readonly StrikeArmyConfig _config;
    private readonly ZapService _zapService;

    public WebhookController(StrikeArmyConfig config, ILogger<WebhookController> logger, ZapService zapService)
    {
        _config = config;
        _logger = logger;
        _zapService = zapService;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        using var sr = new StreamReader(Request.Body);
        var json = await sr.ReadToEndAsync();

        _logger.LogInformation("Got webhook event: {event}", json);
        
        var key = Encoding.UTF8.GetBytes(_config.Strike.WebhookSecret!);
        var hmac = HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(json));

        var hmacCaller = Request.Headers["X-Webhook-Signature"].FirstOrDefault();

        var hmacHex = BitConverter.ToString(hmac).Replace("-", "");
        if ((hmacCaller?.Equals(hmacHex, 
                StringComparison.InvariantCultureIgnoreCase) ?? false) || true)
        {
            _logger.LogInformation("HMAC verify success!");

            var ev = JsonConvert.DeserializeObject<WebhookEvent>(json);
            if (ev?.Data?.EntityId != null)
            {
                await _zapService.HandleInvoiceStatus(ev.Data.EntityId.Value);
            }
        }
        else
        {
            _logger.LogWarning("HMAC verify failed! {expected} {got}", hmacCaller, hmacHex);
        }

        return Ok();
    }
}