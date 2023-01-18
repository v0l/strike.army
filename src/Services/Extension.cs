using System.Security.Claims;
using NBitcoin.DataEncoders;
using NBitcoin.Secp256k1;
using NNostr.Client;
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

    public static string ToHex(this Guid g)
    {
        return g.ToByteArray().ToHex();
    }

    public static Guid ToGuid(this string hex)
    {
        return new(Convert.FromHexString(hex));
    }

    public static string ToHex(this byte[] data)
    {
        return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
    }

    public static byte[] ConvertBits(this byte[] data, int fromBits, int toBits, bool pad = true)
    {
        var acc = 0;
        var bits = 0;
        var maxv = (1 << toBits) - 1;
        var ret = new List<byte>(64);
        foreach (var value in data)
        {
            if ((value >> fromBits) > 0)
                throw new FormatException("Invalid Bech32 string");
            acc = (acc << fromBits) | value;
            bits += fromBits;
            while (bits >= toBits)
            {
                bits -= toBits;
                ret.Add((byte)((acc >> bits) & maxv));
            }
        }
        if (pad)
        {
            if (bits > 0)
            {
                ret.Add((byte)((acc << (toBits - bits)) & maxv));
            }
        }
        else if (bits >= fromBits || (byte)(((acc << (toBits - bits)) & maxv)) != 0)
        {
            throw new FormatException("Invalid Bech32 string");
        }
        return ret.ToArray();
    }

    public static ECPrivKey? GetPrivateKey(this NostrSettings s)
    {
        var bech32 = Bech32Encoder.ExtractEncoderFromString(s.PrivateKey);
        var words = bech32.DecodeDataRaw(s.PrivateKey, out _);
        var keyData = words.ConvertBits(5, 8, false); // bare key, manually call ConvertBits
        return ECPrivKey.TryCreate(keyData, out var key) ? key : null;
    }

    public static string? GetHexPubKey(this NostrSettings s)
    {
        return GetPrivateKey(s)?.CreateXOnlyPubKey().ToHex();
    }
}
