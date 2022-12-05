using StrikeArmy.Database.Model;
using StrikeArmy.StrikeApi;

namespace StrikeArmy.ApiModels;

public record UserProfile(User User, Profile Profile, List<Balance> Balances, long? MinPayment);
