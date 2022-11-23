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
    private readonly StrikeAuthService _authService;

    public UserService(StrikeArmyContext db, StrikeApiSettings strikeApiSettings, ILogger<UserService> logger,
        StrikeAuthService authService)
    {
        _db = db;
        _strikeApiSettings = strikeApiSettings;
        _logger = logger;
        _authService = authService;
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
            .Where(a => a.UserId == user.Id)
            .ToArrayAsync();

        if (!tokens.Any()) return default;

        var latestToken = tokens.MaxBy(a => a.Expires);
        if (latestToken!.Expires < DateTime.UtcNow)
        {
            var newToken = await _authService.RefreshToken(latestToken.RefreshToken);
            if (newToken == default)
            {
                _logger.LogError("Failed to refresh token");
                return default;
            }

            var e = _db.AuthTokens.Add(new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AccessToken = newToken!.AccessToken,
                RefreshToken = newToken.RefreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn)
            });

            await _db.SaveChangesAsync();
            latestToken = e.Entity;
        }

        return new(_strikeApiSettings, latestToken.AccessToken);
    }

    public async Task<WithdrawConfig> AddConfig(WithdrawConfig cfg)
    {
        var e = _db.WithdrawConfigs.Add(cfg);
        await _db.SaveChangesAsync();
        return e.Entity;
    }

    public async Task<WithdrawConfig?> GetWithdrawConfig(Guid id)
    {
        return await _db.WithdrawConfigs
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.ConfigReusable)
            .Include(a => a.Payments)
            .SingleOrDefaultAsync(a => a.Id == id);
    }

    public Task AddPayment(WithdrawConfigPayment payment)
    {
        _db.WithdrawConfigPayments.Add(payment);
        return _db.SaveChangesAsync();
    }

    public async Task UpdatePaymentStatus(Guid id, PaymentStatus status, ulong? routingFee, string? statusMessage = null)
    {
        var entity = await _db.WithdrawConfigPayments
            .SingleAsync(a => a.Id == id);

        entity.RoutingFee = routingFee;
        entity.Status = status;
        entity.StatusMessage = statusMessage;
        await _db.SaveChangesAsync();
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
