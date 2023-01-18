using StrikeArmy.StrikeApi;

namespace StrikeArmy.Services;

public class WebhookSetupService : BackgroundService
{
    private readonly ILogger<WebhookSetupService> _logger;
    private readonly StrikeArmyConfig _config;
    private readonly StrikeApi.StrikeApi _api;

    public WebhookSetupService(ILogger<WebhookSetupService> logger, StrikeArmyConfig config, StrikeApi.StrikeApi api)
    {
        _logger = logger;
        _config = config;
        _api = api;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hookUrl = new Uri(_config.BaseUrl!, "/webhook");

        var hooks = await _api.GetWebhookSubscriptions();
        var hook = hooks?.FirstOrDefault(a => a.Uri == hookUrl);
        if (hook == null)
        {
            _logger.LogInformation("No hook found for: {url}, setting up hook.", hookUrl);
            hook = await _api.CreateWebhook(new()
            {
                Secret = _config.Strike.WebhookSecret,
                Uri = hookUrl,
                Enabled = true,
                EventTypes = new()
                {
                    "invoice.created", "invoice.updated"
                },
                Version = "v1"
            });

            if (hook != null)
            {
                _logger.LogInformation("Webhook registered: Id={id}, Url={url}", hook!.Id, hook!.Uri);
            }
            else
            {
                _logger.LogError("Hook setup failed");
            }
        }
        else
        {
            _logger.LogInformation("Found hook: Id={id}, Create={created}", hook.Id, hook.Created);
        }

        var deleteHooks = hooks?.Where(a => a.Uri != hookUrl);
        foreach (var delHook in deleteHooks ?? Enumerable.Empty<WebhookSubscription>())
        {
            if (delHook?.Id != default)
            {
                await _api.DeleteWebhook(delHook!.Id!.Value);
                _logger.LogInformation("Deleted invalid hook: Id={id}, Url={url}", delHook!.Id, delHook!.Uri);
            }
        }

        var tcs = new TaskCompletionSource();
        stoppingToken.Register(() => tcs.SetResult());

        // wait for exit
        await tcs.Task;

        await _api.DeleteWebhook(hook.Id.Value);
    }
}
