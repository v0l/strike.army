using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public UserController(UserService userService, ProfileCache profileCache)
    {
        _userService = userService;
        _profileCache = profileCache;
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

        var k1 = Guid.NewGuid();
        return await _userService.AddConfig(new()
        {
            Id = k1,
            UserId = user.Id,
            Description = cfg.Description,
            Min = cfg.Min == 0 ? null : cfg.Min,
            Max = cfg.Max == 0 ? null : cfg.Max,
            Type = cfg.Type,
            ConfigReusable = cfg.Type is WithdrawConfigType.Reusable ? new WithdrawConfigReusable
            {
                Interval = cfg.Interval!.Value,
                Limit = cfg.Limit!.Value
            } : null
        });
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
    }
}
