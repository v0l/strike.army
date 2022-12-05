using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Database.Configurations;

public class WithdrawConfigPaymentConfiguration : IEntityTypeConfiguration<WithdrawConfigPayment>
{
    public void Configure(EntityTypeBuilder<WithdrawConfigPayment> builder)
    {
        builder.ToTable("WithdrawConfigPayment");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.StrikeQuoteId)
            .IsRequired();

        builder.Property(a => a.Created)
            .IsRequired();

        builder.Property(a => a.Amount)
            .IsRequired();

        builder.Property(a => a.RoutingFee)
            .IsRequired(false);

        builder.Property(a => a.PayeeNodePubKey)
            .IsRequired();

        builder.Property(a => a.Pr)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.StatusMessage)
            .IsRequired(false);

        builder.HasIndex(a => new {a.Created, a.Status});
    }
}
