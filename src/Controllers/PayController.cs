using System.Globalization;
using System.Text;
using BTCPayServer.Lightning;
using LNURL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NNostr.Client;
using StrikeArmy.Services;
using StrikeArmy.StrikeApi;
using SHA256 = System.Security.Cryptography.SHA256;

namespace StrikeArmy.Controllers;

public class LNURLPayRequestExtended : LNURLPayRequest
{
    [JsonProperty("nostrPubkey")]
    public string NostrPubkey { get; set; }

    [JsonProperty("allowsNostr")]
    public bool AllowsNostr { get; set; }
}

[Route($"{PathBase}/{{user}}")]
public class PayController : Controller
{
    public const string PathBase = "p";

    private readonly ILogger<PayController> _logger;
    private readonly StrikeApi.StrikeApi _api;
    private readonly StrikeArmyConfig _config;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ProfileCache _profileExtension;

    public PayController(ILogger<PayController> logger, StrikeApi.StrikeApi api, StrikeArmyConfig config,
        IMemoryCache cache,
        HttpClient httpClient, ProfileCache profileExtension)
    {
        _logger = logger;
        _api = api;
        _config = config;
        _cache = cache;
        _httpClient = httpClient;
        _profileExtension = profileExtension;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    [HttpGet]
    public async Task<IActionResult> GetPayService([FromRoute] string user, [FromQuery] string? d)
    {
        var baseUrl = _config.BaseUrl ?? new Uri($"{Request.Scheme}://{Request.Host}");
        var id = Guid.NewGuid();

        var profile = await _api.GetProfile(user);
        if (profile == null) return StatusCode(404);

        var avatar = await GetAvatar(profile);

        var metadata = new List<string?[]>()
        {
            new[] {"text/plain", d ?? $"Pay to Strike user: {user}"},
            new[] {"text/identifier", $"{user}@{baseUrl.Host}"}
        };

        if (avatar != null)
        {
            metadata.Add(new[] {"image/png;base64", avatar});
        }

        var req = new LNURLPayRequestExtended()
        {
            Callback = new Uri(baseUrl, $"{PathBase}/{user}/{id}"),
            MaxSendable = LightMoney.Satoshis(10_000_000),
            MinSendable = LightMoney.Satoshis(await _profileExtension.GetMinAmount(profile)),
            Metadata = JsonConvert.SerializeObject(metadata),
            CommentAllowed = 250,
            Tag = "payRequest"
        };

        if (_config.Nostr != default)
        {
            var pubkey = _config.Nostr.GetHexPubKey();
            if (pubkey != default)
            {
                req.NostrPubkey = pubkey;
                req.AllowsNostr = true;
            }
        }

        _cache.Set(id, req, TimeSpan.FromMinutes(10));
        return Json(req);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInvoice([FromRoute] string user, [FromRoute] Guid id,
        [FromQuery] long amount,
        [FromQuery] string? comment = null,
        [FromQuery] string? nostr = null)
    {
        try
        {
            var profile = await _api.GetProfile(user);
            if (!(profile?.CanReceive ?? false))
            {
                throw new InvalidOperationException("Account cannot receive!");
            }

            var invoiceRequest = _cache.Get<LNURLPayRequestExtended>(id);
            if (invoiceRequest == default)
            {
                throw new InvalidOperationException($"Cannot find request for invoice {id}");
            }

            if (invoiceRequest.MinSendable.MilliSatoshi > amount || amount > invoiceRequest.MaxSendable.MilliSatoshi)
            {
                throw new InvalidOperationException("Amount is out of bounds");
            }

            var metadata = JsonConvert.DeserializeObject<List<object[]>>(invoiceRequest.Metadata) ?? new List<object[]>();
            // extract description from metadata
            var description = (string?)metadata.FirstOrDefault(a =>
                a.Length == 2 && a[0] is string && ((string)a[0]).Equals("text/plain", StringComparison.InvariantCultureIgnoreCase))?[1];

            var invoice = await _api.GenerateInvoice(new()
            {
                Amount = new()
                {
                    Amount = LightMoney.MilliSatoshis(amount)
                        .ToUnit(LightMoneyUnit.BTC)
                        .ToString(CultureInfo.InvariantCulture),
                    Currency = Currencies.BTC
                },
                Description = comment ?? description ?? invoiceRequest.Metadata,
                Handle = user
            });

            if (invoice == null) throw new Exception("Failed to get invoice!");

            var isNostr = invoiceRequest.AllowsNostr && !string.IsNullOrEmpty(nostr);
            string hexDescriptionHash;
            if (isNostr)
            {
                var parsedNote = System.Text.Json.JsonSerializer.Deserialize<NostrEvent>(nostr!);
                if (parsedNote?.Kind != 9734)
                {
                    throw new InvalidOperationException("Invalid zap note, kind must be 9734");
                }

                if (!parsedNote.Verify())
                {
                    throw new InvalidOperationException("Zap note sig check failed");
                }

                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(nostr!));
                hexDescriptionHash = Extension.ToHex(hash);
                invoiceRequest.Metadata = nostr;
            }
            else
            {
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(invoiceRequest.Metadata));
                hexDescriptionHash = Extension.ToHex(hash);
            }

            var quote = await _api.GetInvoiceQuote(invoice.InvoiceId, hexDescriptionHash);
            var rsp = new LNURLPayRequest.LNURLPayRequestCallbackResponse()
            {
                Pr = quote!.LnInvoice
            };

            if (isNostr)
            {
                var zap = new ZapNote()
                {
                    Request = invoiceRequest,
                    Invoice = quote.LnInvoice!
                };
                _cache.Set(invoice.InvoiceId, zap, TimeSpan.FromMinutes(10));
            }

            return Json(rsp);
        }
        catch (Exception ex)
        {
            return Json(new LNUrlStatusResponse
            {
                Status = "ERROR",
                Reason = ex.Message
            });
        }
    }

    private async Task<string?> GetAvatar(Profile? profile)
    {
        try
        {
            if (!string.IsNullOrEmpty(profile?.AvatarUrl))
            {
                var imageData = await _httpClient.GetByteArrayAsync(profile.AvatarUrl);
                return Convert.ToBase64String(imageData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load avatar");
        }

        return null;
    }
}
