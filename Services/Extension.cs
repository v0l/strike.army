using System.Security.Claims;

namespace StrikeArmy.Services;

public static class Extension
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var claimSub = context?.User?.Claims?.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claimSub, out var g) ? g : null;
    }
}
