using DistributedTransactions.Domain.Holdings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class PortfolioManagerConfiguration : BaseEntityConfiguration<PortfolioManager>
{
    public override void Configure(EntityTypeBuilder<PortfolioManager> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => q.Name).IsUnique();
    }
}
