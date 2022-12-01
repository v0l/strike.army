using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StrikeArmy.Database.Model;
using StrikeArmy.Services;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.Controllers;

[Authorize]
[Route("user")]
public class UserController : Controller
{
    private readonly UserService _userService;
    private readonly ProfileCache _profileCache;
    private readonly IMemoryCache _cache;
    private readonly StrikeArmyConfig _config;

    public UserController(UserService userService, ProfileCache profileCache, IMemoryCache cache, StrikeArmyConfig config)
    {
        _userService = userService;
        _profileCache = profileCache;
        _cache = cache;
        _config = config;
    }

    [HttpGet]
    public async Task<UserProfile?> GetUser()
    {
        var user = await GetCurrentUser();
        if (user == default) return default;

        var api = await _userService.GetApi(user);
        if (api == default) return default;

        var profile = await _profileCache.GetProfile(user.StrikeUserId);
        var balance = await api.GetBalances();
        var min = await _profileCache.GetMinAmount(profile);
        return new(user, profile!, balance!, min);
    }

    [HttpPost("withdraw-config")]
    public async Task<WithdrawConfig?> AddWithdrawConfig([FromBody] NewWithdrawConfig cfg)
    {
        var user = await GetCurrentUser();
        if (user == default) return default;

        var id = Guid.NewGuid();
        var ret = await _userService.AddConfig(new()
        {
            Id = id,
            UserId = user.Id,
            Description = cfg.Description,
            Min = cfg.Min == 0 ? null : cfg.Min,
            Max = cfg.Max == 0 ? null : cfg.Max,
            Type = cfg.Type,
            ConfigReusable = cfg.Type is WithdrawConfigType.Reusable ? new()
            {
                Interval = cfg.Interval!.Value,
                Limit = cfg.Limit!.Value
            } : null,
            BoltCardConfig = cfg.BoltCard ? new() : null
        });

        return ret;
    }

    [HttpDelete("withdraw-config/{id:guid}")]
    public async Task<IActionResult> DeleteConfig([FromRoute] Guid id)
    {
        var user = await GetCurrentUser();
        if (user == default) return Unauthorized();

        var config = await _userService.GetWithdrawConfig(id);
        if (config == default) return NotFound();

        // check user owns this config
        if (config.UserId != user.Id) return Unauthorized();

        await _userService.DeleteWithdrawConfig(id);
        return Ok();
    }

    [HttpGet("withdraw-config/{id:guid}/bolt-card-setup")]
    public async Task<IActionResult> GetSetupKey([FromRoute] Guid id)
    {
        var user = await GetCurrentUser();
        if (user == default) return Unauthorized();

        var config = await _userService.GetWithdrawConfig(id);
        if (config == default) return NotFound();

        if (config.UserId != user.Id) return Unauthorized();
        if (config.BoltCardConfig is not {SetupKey: null}) return BadRequest();

        var otk = Guid.NewGuid();
        await _userService.SetBoltSetupKey(config.Id, otk);
        return Json(otk);
    }

    [AllowAnonymous]
    [HttpGet("bolt-card-setup/{setupKey:guid}")]
    public async Task<IActionResult> SetupBoltCard([FromRoute] Guid setupKey)
    {
        var config = await _userService.GetWithdrawConfigByBoltSetupKey(setupKey);
        if (config?.BoltCardConfig?.SetupKey == default) return BadRequest();

        var baseUrl = _config.BaseUrl ?? new Uri($"{Request.Scheme}://{Request.Host}");

        var ub = new UriBuilder(baseUrl)
        {
            Scheme = "lnurlw",
            Path = $"/{WithdrawController.PathBase}/{config.Id}"
        };

        var rsp = new BoltCardSetup
        {
            BaseUrl = ub.Uri,
            CardName = config.Id.ToString(),
            K0 = config.BoltCardConfig.K0.ToString().Replace("-", string.Empty),
            K1 = config.BoltCardConfig.K1.ToString().Replace("-", string.Empty),
            K2 = config.BoltCardConfig.K2.ToString().Replace("-", string.Empty),
            K3 = config.BoltCardConfig.K3.ToString().Replace("-", string.Empty),
            K4 = config.BoltCardConfig.K4.ToString().Replace("-", string.Empty)
        };

        await _userService.WipeBoltSetupKey(config.Id);

        return Json(rsp);
    }

    private async Task<User?> GetCurrentUser()
    {
        var uid = Request.HttpContext.GetUserId();
        if (!uid.HasValue) return default;

        return await _userService.GetUser(uid.Value);
    }

    public record UserProfile(User User, Profile Profile, List<Balance> Balances, long? MinPayment);

    public class NewWithdrawConfig
    {
        public WithdrawConfigType Type { get; init; }
        public string Description { get; init; }
        public ulong Min { get; init; }
        public ulong Max { get; init; }
        public WithdrawConfigLimitInterval? Interval { get; init; }
        public ulong? Limit { get; init; }
        public bool BoltCard { get; init; }
    }

    public class BoltCardSetup
    {
        [JsonProperty("protocol_name")]
        public string ProtocolName { get; init; } = "create_bolt_card_response";

        [JsonProperty("protocol_version")]
        public int ProtocolVersion { get; init; } = 1;

        [JsonProperty("card_name")]
        public string CardName { get; init; } = null!;

        [JsonProperty("lnurlw_base")]
        public Uri BaseUrl { get; init; } = null!;

        [JsonProperty("k0")]
        public string K0 { get; init; } = null!;

        [JsonProperty("k1")]
        public string K1 { get; init; } = null!;

        [JsonProperty("k2")]
        public string K2 { get; init; } = null!;

        [JsonProperty("k3")]
        public string K3 { get; init; } = null!;

        [JsonProperty("k4")]
        public string K4 { get; init; } = null!;
    }
}
