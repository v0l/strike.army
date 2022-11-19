using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StrikeArmy.Services;

namespace StrikeArmy.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthController> _logger;
    private readonly StrikeArmyConfig _config;
    private readonly HttpClient _client;
    private readonly UserService _userService;

    public AuthController(StrikeArmyConfig config, HttpClient client, IMemoryCache cache, ILogger<AuthController> logger,
        UserService userService)
    {
        _config = config;
        _client = client;
        _cache = cache;
        _logger = logger;
        _userService = userService;
    }

    private string[] Scopes => new[]
    {
        "partner.balances.read",
        "partner.payment-quote.lightning.create",
        "partner.payment-quote.execute"
    };

    [HttpGet]
    public IActionResult Authorize()
    {
        var state = Guid.NewGuid().ToString();
        var codeVerifier = CreateCodeVerifier();

        _cache.Set(state, codeVerifier);
        var challenge = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);

        var ub = new UriBuilder(new Uri(_config.Strike.AuthEndpoint, "/connect/authorize"))
        {
            Query = string.Join("&",
                BuildAuthorizeQuery(state, challenge).Select(a => $"{a.Key}={Uri.EscapeDataString(a.Value)}"))
        };

        return Redirect(ub.Uri.ToString());
    }

    [Route("token")]
    public async Task<IActionResult> Token([FromQuery] string code, [FromQuery] string state)
    {
        var codeVerifier = _cache.Get<string>(state);
        if (codeVerifier == default)
        {
            return StatusCode(400);
        }

        var form = new FormUrlEncodedContent(BuildTokenQuery(code, codeVerifier));
        var rsp = await _client.PostAsync(new Uri(_config.Strike.AuthEndpoint, "/connect/token"), form);
        var json = await rsp.Content.ReadAsStringAsync();
        if (!rsp.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get token from strike, response: {Json}", json);
            return StatusCode(500);
        }

        _logger.LogInformation("Got token: {Token}", json);

        var token = JsonConvert.DeserializeObject<OAuthAccessToken>(json);
        var user = _userService.CreateUserFromToken(token!.AccessToken);

        // todo: Create local JWT token

        return Redirect("/");
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
    private Dictionary<string, string> BuildAuthorizeQuery(string state, string challenge)
        => new()
        {
            {"response_type", "code"},
            {"client_id", _config.Strike.ClientId},
            {"scope", string.Join(" ", Scopes)},
            {"prompt", "login"},
            {"state", state},
            {"redirect_uri", new Uri(_config.BaseUrl!, "/auth/token").ToString()},
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
            {"client_id", _config.Strike.ClientId},
            {"client_secret", _config.Strike.ClientSecret},
            {"grant_type", "authorization_code"},
            {"code", code},
            {"code_verifier", verifier},
            {"redirect_uri", new Uri(_config.BaseUrl!, "/auth/token").ToString()}
        };

    private class OAuthAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; init; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonProperty("token_type")]
        public string TokenType { get; init; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; init; }

        [JsonProperty("scope")]
        public string Scope { get; init; }

        [JsonProperty("id_token")]
        public string IdToken { get; init; }
    }
}
