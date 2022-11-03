using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using BTCPayServer.Lightning;
using LNURL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.Controllers;

[Route($"{PathBase}/{{user}}")]
public class PayController : Controller
{
    public const string PathBase = "pay";

    private readonly ILogger<PayController> _logger;
    private readonly StrikeApi.StrikeApi _api;
    private readonly StrikeArmyConfig _config;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;

    public PayController(ILogger<PayController> logger, StrikeApi.StrikeApi api, StrikeArmyConfig config,
        IMemoryCache cache,
        HttpClient httpClient)
    {
        _logger = logger;
        _api = api;
        _config = config;
        _cache = cache;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    [HttpGet]
    public async Task<IActionResult> GetPayService([FromRoute] string user, [FromQuery] string? description)
    {
        var baseUrl = _config?.BaseUrl ?? new Uri($"{Request.Scheme}://{Request.Host}");
        var id = Guid.NewGuid();

        var profile = await _api.GetProfile(user);
        if (profile == null) return StatusCode(404);

        var avatar = await getAvatar(profile);

        var metadata = new List<string?[]>()
        {
            new[] {"text/plain", description ?? $"Pay to Strike user: {user}"},
            new[] {"text/identifier", $"{user}@{baseUrl.Host}"}
        };

        if (avatar != null)
        {
            metadata.Add(new[] {"image/png;base64", avatar});
        }

        var req = new LNURLPayRequest()
        {
            Callback = new Uri(baseUrl, $"{PathBase}/{user}/{id}"),
            MaxSendable = LightMoney.Satoshis(10_000_000),
            MinSendable = LightMoney.Satoshis(await getMinAmount(profile)),
            Metadata = JsonConvert.SerializeObject(metadata),
            CommentAllowed = 250,
            Tag = "payRequest"
        };

        _cache.Set(id, req, TimeSpan.FromMinutes(10));
        return Content(JsonConvert.SerializeObject(req), "application/json");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInvoice([FromRoute] string user, [FromRoute] Guid id,
        [FromQuery] long amount,
        [FromQuery] string? comment)
    {
        try
        {
            var profile = await _api.GetProfile(user);
            if (!(profile?.CanReceive ?? false))
            {
                throw new InvalidOperationException("Account cannot receive!");
            }

            var invoiceRequest = _cache.Get<LNURLPayRequest>(id);
            if (invoiceRequest == default)
            {
                throw new InvalidOperationException($"Cannot find request for invoice {id}");
            }

            var metadata = JsonConvert.DeserializeObject<List<string[]>>(invoiceRequest.Metadata);
            // extract description from metadata
            var description = metadata?.FirstOrDefault(a =>
                a.Length == 2 && a[0].Equals("text/plain", StringComparison.InvariantCultureIgnoreCase))?[1];

            var invoice = await _api.GenerateInvoice(new()
            {
                Amount = new()
                {
                    Amount = LightMoney.MilliSatoshis(amount)
                        .ToUnit(LightMoneyUnit.BTC)
                        .ToString(CultureInfo.InvariantCulture),
                    Currency = Currencies.BTC
                },
                CorrelationId = id.ToString(),
                Description = comment ?? description ?? invoiceRequest.Metadata,
                Handle = user
            });

            if (invoice == null) throw new Exception("Failed to get invoice!");

            var descriptionHashData = SHA256.HashData(Encoding.UTF8.GetBytes(invoiceRequest.Metadata));
            var hexDescriptionHash = BitConverter.ToString(descriptionHashData).Replace("-", string.Empty).ToLower();
            var quote = await _api.GetInvoiceQuote(invoice.InvoiceId, hexDescriptionHash);
            var rsp = new LNURLPayRequest.LNURLPayRequestCallbackResponse()
            {
                Pr = quote.LnInvoice
            };

            return Content(JsonConvert.SerializeObject(rsp), "application/json");
        }
        catch (Exception ex)
        {
            return Content(JsonConvert.SerializeObject(new
            {
                Status = "ERROR",
                Reason = ex.Message
            }));
        }
    }

    private async Task<string?> getAvatar(Profile? profile)
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

    private async Task<long> getMinAmount(Profile? profile)
    {
        const long defaultMin = 1_000;
        var currency = profile?.Currencies.FirstOrDefault(a => a.IsDefault)?.Currency ?? Currencies.USD;
        var rates = await getRate(currency);
        var minAmount = currency switch
        {
            Currencies.BTC => 1e-8m,
            Currencies.USD or Currencies.EUR or Currencies.GBP or Currencies.USDT => 0.01m,
            _ => throw new ArgumentOutOfRangeException()
        };
        return rates != default ? (long) (minAmount / rates.Amount * 1e8m) : defaultMin;
    }

    private async ValueTask<ConversionRate?> getRate(Currencies toCurrency)
    {
        const string ratesKey = "rates";
        var rates = _cache.Get<List<ConversionRate>>(ratesKey);
        if (rates == default)
        {
            rates = await _api.GetRates();
            _cache.Set(ratesKey, rates, TimeSpan.FromMinutes(10));
        }

        var rate = rates?.FirstOrDefault(a => a.Target == toCurrency && a.Source == Currencies.BTC);
        if (rate != default)
        {
            return rate;
        }

        // look for opposite and invert amount
        var rateInverted = rates?.FirstOrDefault(a => a.Source == toCurrency && a.Target == Currencies.BTC);
        if (rateInverted != default)
        {
            return new()
            {
                Amount = 1 / rateInverted.Amount,
                Source = rateInverted.Source,
                Target = rateInverted.Target
            };
        }

        return default;
    }
}