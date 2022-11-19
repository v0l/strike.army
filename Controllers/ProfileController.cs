using Microsoft.AspNetCore.Mvc;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.Controllers;

[Route("profile/{user}")]
public class ProfileController : Controller
{
    private readonly ProfileCache _profileCache;
    private readonly HttpClient _client;

    public ProfileController(ProfileCache profileCache, HttpClient client)
    {
        _profileCache = profileCache;
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile([FromRoute] string user)
    {
        var profile = await _profileCache.GetProfile(user);
        if (profile == default)
        {
            return StatusCode(404);
        }

        return Json(profile);
    }

    [HttpGet("avatar")]
    public async Task GetAvatar([FromRoute] string user)
    {
        var profile = await _profileCache.GetProfile(user);
        if (profile == default)
        {
            Response.StatusCode = 404;
            return;
        }

        if (!string.IsNullOrEmpty(profile.AvatarUrl))
        {
            var imageData = await _client.GetByteArrayAsync(new Uri(profile.AvatarUrl!));
            Response.ContentType = "image/png";
            await Response.Body.WriteAsync(imageData, 0, imageData.Length);
        }
    }
}
