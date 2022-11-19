using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .IsRequired();

        builder.Property(a => a.StrikeUserId)
            .IsRequired();

        builder.Property(a => a.Created)
            .IsRequired();
    }
}
