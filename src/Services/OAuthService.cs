using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace StrikeArmy.Services;

public abstract class OAuthService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _client;

    public OAuthService(HttpClient client, IMemoryCache cache)
    {
        _cache = cache;
        _client = client;
    }

    protected abstract string[] Scopes { get; }

    protected abstract Uri BaseUri { get; }
    protected abstract Uri RedirectUri { get; }
    protected abstract string ClientId { get; }
    protected abstract string ClientSecret { get; }

    public Uri Authorize()
    {
        var state = Guid.NewGuid();
        var codeVerifier = CreateCodeVerifier();

        _cache.Set(state, codeVerifier);
        var challenge = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);

        var ub = new UriBuilder(new Uri(BaseUri, "/connect/authorize"))
        {
            Query = string.Join("&",
                BuildAuthorizeQuery(state, challenge).Select(a => $"{a.Key}={Uri.EscapeDataString(a.Value)}"))
        };

        return ub.Uri;
    }

    public async Task<OAuthAccessToken?> GetToken(Guid state, string code)
    {
        var codeVerifier = _cache.Get<string>(state);
        if (codeVerifier == default)
        {
            throw new Exception("Invalid state");
        }

        var form = new FormUrlEncodedContent(BuildTokenQuery(code, codeVerifier));
        var rsp = await _client.PostAsync(new Uri(BaseUri, "/connect/token"), form);
        var json = await rsp.Content.ReadAsStringAsync();
        if (!rsp.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get token: {json}");
        }

        Console.WriteLine(json);
        return JsonConvert.DeserializeObject<OAuthAccessToken>(json);
    }

    public async Task<OAuthAccessToken?> RefreshToken(string token)
    {
        var form = new FormUrlEncodedContent(BuildRefreshTokenQuery(token));
        var rsp = await _client.PostAsync(new Uri(BaseUri, "/connect/token"), form);
        var json = await rsp.Content.ReadAsStringAsync();
        if (!rsp.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to refresh token: {json}");
        }

        Console.WriteLine(json);
        return JsonConvert.DeserializeObject<OAuthAccessToken>(json);
    }
    
    private string CreateCodeVerifier(int len = 128)
    {
        const string chars = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321-._~";
        var ret = new char[len];
        for (var x = 0; x < len; x++)
        {
            var c = Random.Shared.Next(0, chars.Length);
            ret[x] = chars[c];
        }

        return new string(ret);
    }

    /// <summary>
    /// Build query args for authorize
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> BuildAuthorizeQuery(Guid state, string challenge)
        => new()
        {
            {"response_type", "code"},
            {"client_id", ClientId},
            {"scope", string.Join(" ", Scopes)},
            {"prompt", "login"},
            {"state", state.ToString()},
            {"redirect_uri", RedirectUri.ToString()},
            {"code_challenge", challenge},
            {"code_challenge_method", "S256"}
        };

    /// <summary>
    /// Build query args for token generation
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> BuildTokenQuery(string code, string verifier)
        => new()
        {
            {"client_id", ClientId},
            {"client_secret", ClientSecret},
            {"grant_type", "authorization_code"},
            {"code", code},
            {"code_verifier", verifier},
            {"redirect_uri", RedirectUri.ToString()}
        };

    private Dictionary<string, string> BuildRefreshTokenQuery(string token)
        => new()
        {
            {"client_id", ClientId},
            {"client_secret", ClientSecret},
            {"grant_type", "refresh_token"},
            {"refresh_token", token}
        };

    public class OAuthAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; init; } = null!;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonProperty("token_type")]
        public string TokenType { get; init; } = null!;

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; init; } = null!;

        [JsonProperty("scope")]
        public string Scope { get; init; } = null!;

        [JsonProperty("id_token")]
        public string IdToken { get; init; } = null!;
    }
}
