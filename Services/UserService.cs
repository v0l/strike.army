using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using StrikeArmy.Database;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Services;

public class UserService
{
    private readonly StrikeArmyContext _db;
    private readonly StrikeApi.StrikeApi _api;

    public UserService(StrikeArmyContext db, StrikeApi.StrikeApi api)
    {
        _db = db;
        _api = api;
    }

    public async Task<User> CreateUserFromToken(string accessToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var uid = Guid.Parse(jwt.Payload.Sub);

        var existingUser = await _db.Users.FirstOrDefaultAsync(a => a.StrikeUserId == uid);
        if (existingUser != default)
        {
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
                    AccessToken = accessToken,
                    Created = jwt.IssuedAt,
                    Expires = jwt.ValidTo
                }
            }
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        throw new Exception();
    }
}
