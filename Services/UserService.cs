using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StrikeArmy.Database;
using StrikeArmy.Database.Model;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.Services;

public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly StrikeApiSettings _strikeApiSettings;
    private readonly StrikeArmyContext _db;

    public UserService(StrikeArmyContext db, StrikeApiSettings strikeApiSettings, ILogger<UserService> logger)
    {
        _db = db;
        _strikeApiSettings = strikeApiSettings;
        _logger = logger;
    }

    public Task<User?> GetUser(Guid id)
    {
        return _db.Users
            .Include(a => a.WithdrawConfigs)
            .SingleOrDefaultAsync(a => a.Id == id);
    }

    public async Task<StrikeApi.StrikeApi?> GetApi(User user)
    {
        var tokens = await _db.AuthTokens
            .AsNoTracking()
            .Where(a => a.UserId == user.Id && DateTime.UtcNow < a.Expires)
            .ToArrayAsync();

        if (!tokens.Any()) return default;

        var latestToken = tokens.MaxBy(a => a.Expires);
        if (latestToken!.Expires < DateTime.UtcNow)
        {
            _logger.LogWarning("token expired");
        }

        return new(_strikeApiSettings, tokens.MaxBy(a => a.Expires)!.AccessToken);
    }

    public async Task<User> CreateUserFromToken(OAuthService.OAuthAccessToken token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        var uid = Guid.Parse(jwt.Payload.Sub);

        var existingUser = await _db.Users.FirstOrDefaultAsync(a => a.StrikeUserId == uid);
        if (existingUser != default)
        {
            _db.AuthTokens.Add(new()
            {
                Id = Guid.NewGuid(),
                UserId = existingUser.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                Created = jwt.IssuedAt,
                Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
            });

            await _db.SaveChangesAsync();
            return existingUser;
        }

        var newUserId = Guid.NewGuid();
        var newUser = new User
        {
            Id = newUserId,
            StrikeUserId = uid,
            Created = DateTime.UtcNow,
            AuthTokens = new()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = newUserId,
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    Created = jwt.IssuedAt,
                    Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
                }
            }
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        return newUser;
    }

    public ClaimsPrincipal CreateLoginPrincipal(User user, DateTime expiry)
    {
        var identity = new ClaimsIdentity(CreateClaims(user, expiry), "login");
        return new ClaimsPrincipal(identity);
    }

    private IEnumerable<Claim> CreateClaims(User user, DateTime expiry)
    {
        return new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Expiration, new DateTimeOffset(expiry).ToUnixTimeSeconds().ToString())
        };
    }
}
