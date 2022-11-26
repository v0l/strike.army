using System.Security.Claims;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Services;

public static class Extension
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var claimSub = context.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claimSub, out var g) ? g : null;
    }

    public static ulong? GetRemainingUsage(this WithdrawConfig config)
    {
        if (config.Type == WithdrawConfigType.SingleUse)
        {
            var paid = config.Payments.Any(a => a.Status is PaymentStatus.Paid);
            return paid ? 0 : config.Max;
        }

        var window = config.ConfigReusable?.Interval switch
        {
            WithdrawConfigLimitInterval.Daily => DateTime.UtcNow.AddDays(-1),
            WithdrawConfigLimitInterval.Weekly => DateTime.UtcNow.AddDays(-7),
            _ => throw new Exception("Invalid interval")
        };

        var used = config.Payments
            .Where(a => a.Created > window &&
                        a.Status is PaymentStatus.Paid or PaymentStatus.Pending)
            .Sum(a => (long)a.Amount + (long)(a.RoutingFee ?? 0));

        var limit = config.ConfigReusable!.Limit;
        return Math.Max(0, limit - (ulong)used);
    }
}
