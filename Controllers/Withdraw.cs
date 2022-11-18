using BTCPayServer.Lightning;
using LNURL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.Controllers;

[Route($"{WithdrawBase}/{{user}}")]
public class Withdraw : Controller
{
    private const string WithdrawBase = "withdraw";
    private readonly StrikeArmyConfig _config;
    private readonly StrikeApi.StrikeApi _api;
    private readonly IMemoryCache _cache;
    private readonly ProfileExtension _profileExtension;

    public Withdraw(StrikeArmyConfig config, StrikeApi.StrikeApi api, IMemoryCache cache, ProfileExtension profileExtension)
    {
        _config = config;
        _api = api;
        _cache = cache;
        _profileExtension = profileExtension;
    }

    [HttpGet]
    public async Task<IActionResult> GetService([FromRoute] string user, [FromQuery] string? description = null,
        [FromQuery] Guid? secret = null)
    {
        var baseUrl = _config.BaseUrl ?? new Uri($"{Request.Scheme}://{Request.Host}");
        var id = Guid.NewGuid();
        try
        {
            var profile = await _api.GetProfile(user);
            if (profile == null) return StatusCode(404);

            if (_config.Secret != secret) return StatusCode(401);

            var amt = await _profileExtension.GetMinAmount(profile);
            var svc = new LNURLWithdrawRequest
            {
                Tag = "withdrawRequest",
                Callback = new(baseUrl, $"/{WithdrawBase}/{user}/execute"),
                MinWithdrawable = LightMoney.Satoshis(amt),
                MaxWithdrawable = LightMoney.Satoshis(amt),
                K1 = id.ToString(),
                DefaultDescription = description ?? "not used"
            };

            _cache.Set(id.ToString(), svc, TimeSpan.FromMinutes(200));
            return Json(svc);
        }
        catch (Exception ex)
        {
            return Json(new LNUrlStatusResponse()
            {
                Status = "ERROR",
                Reason = ex.Message
            });
        }
    }

    [HttpGet("execute")]
    public async Task<IActionResult> ExecuteWithdraw([FromRoute] string user, [FromQuery] string k1, [FromQuery] string pr)
    {
        try
        {
            if (string.IsNullOrEmpty(k1) || string.IsNullOrEmpty(pr))
            {
                throw new InvalidOperationException("K1 or PR is not set");
            }

            var svc = _cache.Get<LNURLWithdrawRequest>(k1);
            if (svc == default)
            {
                throw new InvalidOperationException("K1 invalid");
            }

            var invoice = BOLT11PaymentRequest.Parse(pr, Network.Main); // todo: detect network
            if (invoice.MinimumAmount < svc.MinWithdrawable || invoice.MinimumAmount > svc.MaxWithdrawable)
            {
                throw new Exception("Amount is out of range");
            }

            var quotePay = await _api.QuotePayInvoice(pr);
            if (quotePay == default)
            {
                throw new Exception("Invalid pay invoice response");
            }

            var payInvoice = await _api.ExecutePayInvoice(quotePay.PaymentQuoteId);
            if (payInvoice?.Result.Equals("SUCCESS") ?? false)
            {
                return Json(new LNUrlStatusResponse
                {
                    Status = "OK"
                });
            }

            return Json(new LNUrlStatusResponse
            {
                Status = "ERROR",
                Reason = $"Invalid status: {payInvoice?.Result}"
            });
        }
        catch (Exception ex)
        {
            return Json(new LNUrlStatusResponse()
            {
                Status = "ERROR",
                Reason = ex.Message
            });
        }
    }
}
