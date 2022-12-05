using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StrikeArmy.Services;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly UserService _userService;
    private readonly StrikeAuthService _authService;

    public AuthController(UserService userService, StrikeAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }


    [HttpGet]
    public IActionResult Authorize()
    {
        var url = _authService.Authorize();
        return Redirect(url.ToString());
    }

    [Route("token")]
    public async Task<IActionResult> Token([FromQuery] string code, [FromQuery] Guid state)
    {
        var strikeToken = await _authService.GetToken(state, code);
        var user = await _userService.CreateUserFromToken(strikeToken!);

        var expire = DateTime.UtcNow.AddDays(7);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            _userService.CreateLoginPrincipal(user, expire), new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = expire,
                IsPersistent = true
            });

        return Redirect("/account");
    }
}
