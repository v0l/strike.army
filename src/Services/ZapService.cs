using Microsoft.Extensions.Caching.Memory;
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
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public ZapService(ILogger<ZapService> logger, StrikeArmyConfig config, IMemoryCache cache, StrikeApi.StrikeApi api)
    {
        _logger = logger;
        _config = config;
        _cache = cache;
        _api = api;
    }

    public async Task HandleInvoiceStatus(Guid id)
    {
        await Lock.WaitAsync();
        try
        {
            var req = _cache.Get<ZapNote>(id);
            if (req == default) return;

            var invoice = await _api.GetInvoice(id);
            if (invoice == default)
            {
                _logger.LogWarning("No invoice found {id}", id);
                return;
            }

            var zapNote = JsonSerializer.Deserialize<NostrEvent>(req.Request.Metadata);
            if (zapNote == default)
            {
                _logger.LogWarning("Could not parse zap note {note}", req.Request.Metadata);
                return;
            }

            var pubkey = _config.Nostr?.GetHexPubKey();
            if (invoice.State == InvoiceState.PAID && pubkey != default)
            {
                var tags = zapNote.Tags.Where(a => a.TagIdentifier.Length == 1).ToList();
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
                await Task.WhenAll(_config.Nostr!.Relays.Concat(taggedRelays).Distinct().Select(async relay =>
                {
                    try
                    {
                        var cts = new CancellationTokenSource();
                        cts.CancelAfter(TimeSpan.FromSeconds(30));
                        
                        using var c = new NostrClient(new Uri(relay));
                        await c.ConnectAndWaitUntilConnected(cts.Token);
                        await c.PublishEvent(zapReceipt, cts.Token);
                        var rsp = c.ListenForRawMessages().GetAsyncEnumerator(cts.Token);
                        await rsp.MoveNextAsync();
                        _logger.LogInformation("[{relay}] Response: {message}", relay, rsp.Current);
                        await c.Disconnect();
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning("[{relay}] Failed to send zap receipt {message}", relay, e.Message);
                    }
                }));

                _cache.Remove(id);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to handle zap");
        }
        finally
        {
            Lock.Release();
        }
    }
}

public sealed class ZapNote
{
    public LNURLPayRequestExtended Request { get; init; } = null!;
    public string Invoice { get; init; } = null!;
}
