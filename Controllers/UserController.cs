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
        return new(user, profile!, balance!);
    }

    private async Task<User?> GetCurrentUser()
    {
        var uid = Request.HttpContext.GetUserId();
        if (!uid.HasValue) return default;

        return await _userService.GetUser(uid.Value);
    }

    public record UserProfile(User User, Profile Profile, List<Balance> Balances);
}
