using BTCPayServer.Lightning;
using LNURL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using StrikeArmy.Database.Model;
using StrikeArmy.Services;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.Controllers;

[Route(WithdrawBase)]
public class Withdraw : Controller
{
    private const string WithdrawBase = "withdraw";
    private readonly StrikeArmyConfig _config;
    private readonly IMemoryCache _cache;
    private readonly ProfileCache _profileExtension;
    private readonly UserService _userService;

    public Withdraw(StrikeArmyConfig config, IMemoryCache cache, ProfileCache profileExtension,
        UserService userService)
    {
        _config = config;
        _cache = cache;
        _profileExtension = profileExtension;
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetService([FromRoute] Guid id)
    {
        var oneTimeSecret = Guid.NewGuid();
        var baseUrl = _config.BaseUrl ?? new Uri($"{Request.Scheme}://{Request.Host}");
        try
        {
            var (config, remaining) = await LoadConfig(id);
            var profile = await LoadProfile(config.User.StrikeUserId);

            var minAmount = config.Min ?? (ulong)await _profileExtension.GetMinAmount(profile);
            var maxAmount = Math.Min(config.Max ?? 0, remaining ?? 0);
            var svc = new LNURLWithdrawRequest
            {
                Tag = "withdrawRequest",
                Callback = new(baseUrl, $"/{WithdrawBase}/execute"),
                MinWithdrawable = LightMoney.Satoshis(minAmount),
                MaxWithdrawable = LightMoney.Satoshis(maxAmount),
                K1 = oneTimeSecret.ToString(),
                DefaultDescription = config.Description,
                PayLink = new(baseUrl, $"/pay/{profile.Handle}")
            };

            _cache.Set(oneTimeSecret, id, TimeSpan.FromMinutes(5));
            _cache.Set($"{oneTimeSecret}-req", svc, TimeSpan.FromMinutes(5));
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
    public async Task<IActionResult> ExecuteWithdraw([FromQuery] Guid k1, [FromQuery] string pr)
    {
        try
        {
            if (string.IsNullOrEmpty(pr) || k1 == Guid.Empty)
            {
                throw new InvalidOperationException("K1 or PR is not set");
            }

            var invoice = ParseInvoice(pr);
            var configId = LoadService(k1, invoice);
            var (config, _) = await LoadConfig(configId, (ulong)invoice.MinimumAmount.ToUnit(LightMoneyUnit.Satoshi));

            var api = await _userService.GetApi(config.User);
            if (api == default)
            {
                throw new Exception("Invalid user account, please login again");
            }

            var (quotePay, internalPaymentId) = await QuotePay(api, invoice, config);
            var (status, payInvoice) = await ExecuteQuotePay(api, quotePay.PaymentQuoteId, internalPaymentId);
            if (status is PaymentStatus.Paid)
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

    private async Task<Profile> LoadProfile(Guid user)
    {
        var profile = await _profileExtension.GetProfile(user);
        if (profile == default) throw new Exception("User not found");

        return profile;
    }

    private BOLT11PaymentRequest ParseInvoice(string pr)
    {
        Network? net;
        if (pr.StartsWith("lnbc"))
        {
            net = Network.Main;
        }
        else if (pr.StartsWith("lntb"))
        {
            net = Network.TestNet;
        }
        else if (pr.StartsWith("lnbcrt"))
        {
            net = Network.RegTest;
        }
        else
        {
            throw new Exception("Invalid invoice");
        }

        return BOLT11PaymentRequest.Parse(pr, net);
    }

    private Guid LoadService(Guid k1, BOLT11PaymentRequest invoice)
    {
        var id = _cache.Get<Guid>(k1);
        if (id == default)
        {
            throw new InvalidOperationException("K1 invalid");
        }

        var svc = _cache.Get<LNURLWithdrawRequest>($"{k1}-req");
        _cache.Remove(k1);
        _cache.Remove($"{k1}-req");

        // Check limits service
        if (invoice.MinimumAmount < svc!.MinWithdrawable || invoice.MinimumAmount > svc.MaxWithdrawable)
        {
            throw new Exception("Amount is out of range");
        }

        return id;
    }

    private async Task<(WithdrawConfig Config, ulong? Remaining)> LoadConfig(Guid configId, ulong payAmount = 0)
    {
        // Load config
        var config = await _userService.GetWithdrawConfig(configId);
        if (config == default) throw new Exception("Invalid withdraw config");

        // Check config limits
        var remaining = config.Remaining;
        if (payAmount > remaining || remaining == 0)
        {
            throw new Exception("Quota exhausted");
        }

        return (config, remaining);
    }

    private async Task<(QuotePayInvoiceResponse QuotePayResponse, Guid InternalPaymentId)> QuotePay(StrikeApi.StrikeApi api,
        BOLT11PaymentRequest invoice, WithdrawConfig config)
    {
        var pr = invoice.ToString();
        var quotePay = await api.QuotePayInvoice(pr);
        if (quotePay == default)
        {
            throw new Exception("Invalid pay invoice response");
        }

        var internalPaymentId = Guid.NewGuid();
        await _userService.AddPayment(new()
        {
            Id = internalPaymentId,
            WithdrawConfigId = config.Id,
            StrikeQuoteId = quotePay.PaymentQuoteId,
            Amount = (ulong)invoice.MinimumAmount.ToUnit(LightMoneyUnit.Satoshi),
            PayeeNodePubKey = invoice.GetPayeePubKey().ToHex(),
            Pr = pr,
            Status = PaymentStatus.New
        });

        return (quotePay, internalPaymentId);
    }

    private async Task<(PaymentStatus Status, ExecutePayInvoiceResponse? ExecuteResponse)> ExecuteQuotePay(StrikeApi.StrikeApi api,
        Guid quoteId, Guid internalPaymentId)
    {
        var payInvoice = await api.ExecutePayInvoice(quoteId);
        var status = payInvoice?.Result switch
        {
            "SUCCESS" => PaymentStatus.Paid,
            "PENDING" => PaymentStatus.Pending,
            _ => PaymentStatus.Failed
        };

        await _userService.UpdatePaymentStatus(internalPaymentId, status,
            !string.IsNullOrEmpty(payInvoice?.NetworkFee?.Amount) ? ulong.Parse(payInvoice.NetworkFee!.Amount) : null,
            payInvoice?.Result);

        return (status, payInvoice);
    }
}
