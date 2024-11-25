using DistributedTransactions.Domain.Holdings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class PortfolioConfiguration : BaseEntityConfiguration<Portfolio>
{
    public override void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => q.AccountNum).IsUnique();
        builder.Property(e => e.Cash)
            .HasColumnType("numeric(17, 2)");
    }
}
