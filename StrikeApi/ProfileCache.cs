using Microsoft.Extensions.Caching.Memory;

namespace StrikeArmy.StrikeApi;

public class ProfileCache
{
    private readonly StrikeApi _api;
    private readonly IMemoryCache _cache;

    public ProfileCache(StrikeApi api, IMemoryCache cache)
    {
        _api = api;
        _cache = cache;
    }

    public async Task<Profile?> GetProfile(string username)
    {
        var key = $"profile:{username}";
        var profile = _cache.Get<Profile>(key);
        if (profile == default)
        {
            profile = await _api.GetProfile(username);
            if (profile != default)
            {
                _cache.Set(key, profile, TimeSpan.FromMinutes(10));
            }
        }

        return profile;
    }

    /// <summary>
    /// Get min amount for send/receive in sats
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<long> GetMinAmount(Profile? profile)
    {
        const long defaultMin = 1_000;
        var currency = profile?.Currencies.FirstOrDefault(a => a.IsDefault)?.Currency ?? Currencies.USD;
        var rates = await GetRate(currency);
        var minAmount = currency switch
        {
            Currencies.BTC => 1e-8m,
            Currencies.USD or Currencies.EUR or Currencies.GBP or Currencies.USDT => 0.01m,
            _ => throw new ArgumentOutOfRangeException()
        };

        return rates != default ? (long)Math.Ceiling(minAmount / rates.Amount * 1e8m) : defaultMin;
    }

    private async ValueTask<ConversionRate?> GetRate(Currencies toCurrency)
    {
        const string ratesKey = "rates";
        var rates = _cache.Get<List<ConversionRate>>(ratesKey);
        if (rates == default)
        {
            rates = await _api.GetRates();
            _cache.Set(ratesKey, rates, TimeSpan.FromMinutes(1));
        }

        var rate = rates?.FirstOrDefault(a => a.Target == toCurrency && a.Source == Currencies.BTC);
        if (rate != default)
        {
            return rate;
        }

        // look for opposite and invert amount
        var rateInverted = rates?.FirstOrDefault(a => a.Source == toCurrency && a.Target == Currencies.BTC);
        if (rateInverted != default)
        {
            return new()
            {
                Amount = 1 / rateInverted.Amount,
                Source = rateInverted.Source,
                Target = rateInverted.Target
            };
        }

        return default;
    }
}
