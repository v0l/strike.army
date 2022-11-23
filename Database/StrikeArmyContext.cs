using Microsoft.EntityFrameworkCore;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Database;

public class StrikeArmyContext : DbContext
{
    public StrikeArmyContext()
    {
    }

    public StrikeArmyContext(DbContextOptions<StrikeArmyContext> ctx) : base(ctx)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuthToken> AuthTokens => Set<AuthToken>();
    public DbSet<WithdrawConfig> WithdrawConfigs => Set<WithdrawConfig>();
    public DbSet<WithdrawConfigPayment> WithdrawConfigPayments => Set<WithdrawConfigPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(User).Assembly);
    }
}
