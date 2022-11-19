using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Database.Configurations;

public class AuthTokenConfiguration : IEntityTypeConfiguration<AuthToken>
{
    public void Configure(EntityTypeBuilder<AuthToken> builder)
    {
        builder.ToTable("AuthToken");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany(a => a.AuthTokens)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.AccessToken)
            .IsRequired();

        builder.Property(a => a.Expires)
            .IsRequired();

        builder.Property(a => a.Created)
            .IsRequired();
    }
}
