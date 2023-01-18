using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NNostr.Client;
using StrikeArmy.Controllers;
using StrikeArmy.StrikeApi;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StrikeArmy.Services;

public class ZapService
{
    private readonly ILogger<ZapService> _logger;
    private readonly StrikeArmyConfig _config;
    private readonly IMemoryCache _cache;
    private readonly StrikeApi.StrikeApi _api;

    public ZapService(ILogger<ZapService> logger, StrikeArmyConfig config, IMemoryCache cache, StrikeApi.StrikeApi api)
    {
        _logger = logger;
        _config = config;
        _cache = cache;
        _api = api;
    }

    public async Task HandleInvoiceStatus(Guid id)
    {
        var req = _cache.Get<ZapNote>(id);
        if (req == default) return;

        var invoice = await _api.GetInvoice(id);
        if (invoice == default)
        {
            _logger.LogWarning("No invoice found {id}", id);
            return;
        }

        var metadata = JsonConvert.DeserializeObject<List<string[]>>(req.Request.Metadata);
        if (metadata == default)
        {
            _logger.LogWarning("Could not parse {metadata}", req.Request.Metadata);
            return;
        }

        var zapNoteContent = metadata.FirstOrDefault(a => a[0] == "application/nostr")?[1];
        if (string.IsNullOrEmpty(zapNoteContent))
        {
            _logger.LogWarning("No nostr note found in {metadata}", req.Request.Metadata);
            return;
        }

        var zapNote = JsonSerializer.Deserialize<NostrEvent>(zapNoteContent);
        if (zapNote == default)
        {
            _logger.LogWarning("Could not parse zap note {note}", zapNoteContent);
            return;
        }

        var pubkey = _config.Nostr?.GetHexPubKey();
        if (invoice.State == InvoiceState.PAID && pubkey != default)
        {
            var tags = zapNote.Tags.Where(a => a.TagIdentifier is "e" or "p").ToList();
            tags.Add(new()
            {
                TagIdentifier = "bolt11",
                Data = new() {req.Invoice}
            });

            tags.Add(new()
            {
                TagIdentifier = "description",
                Data = new() {req.Request.Metadata}
            });

            tags.Add(new()
            {
                TagIdentifier = "preimage",
                Data = new() {""}
            });

            var zapReceipt = new NostrEvent()
            {
                Kind = 9735,
                CreatedAt = DateTimeOffset.UtcNow,
                PublicKey = pubkey,
                Content = zapNote.Content,
                Tags = tags
            };

            await zapReceipt.ComputeIdAndSign(_config.Nostr!.GetPrivateKey()!);
            var jsonZap = JsonSerializer.Serialize(zapReceipt);
            _logger.LogInformation("Created tip receipt {json}", jsonZap);

            var taggedRelays = zapNote.Tags.Where(a => a.TagIdentifier == "relays").SelectMany(b => b.Data.Skip(1));
            foreach (var relay in _config.Nostr!.Relays.Concat(taggedRelays).Distinct())
            {
                try
                {
                    using var c = new NostrClient(new Uri(relay));
                    await c.ConnectAndWaitUntilConnected();
                    await c.PublishEvent(zapReceipt);
                    var rsp = c.ListenForRawMessages().GetAsyncEnumerator();
                    await rsp.MoveNextAsync();
                    _logger.LogInformation(rsp.Current);
                    await c.Disconnect();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to send zap receipt");
                }
            }
        }
    }
}

public sealed class ZapNote
{
    public LNURLPayRequestExtended Request { get; init; } = null!;
    public string Invoice { get; init; } = null!;
}
