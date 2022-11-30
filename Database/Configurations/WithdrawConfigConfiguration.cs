using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Database.Configurations;

public class WithdrawConfigConfiguration : IEntityTypeConfiguration<WithdrawConfig>
{
    public void Configure(EntityTypeBuilder<WithdrawConfig> builder)
    {
        builder.ToTable("WithdrawConfig");

        builder.Property(a => a.Id)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany(a => a.WithdrawConfigs)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Min)
            .IsRequired(false);

        builder.Property(a => a.Max)
            .IsRequired(false);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.HasMany(a => a.Payments)
            .WithOne()
            .HasForeignKey(a => a.WithdrawConfigId);

        builder.OwnsOne(a => a.ConfigReusable);
        builder.OwnsOne(a => a.BoltCardConfig);
    }
}
