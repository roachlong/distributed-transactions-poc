using DistributedTransactions.Domain.Market;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class PriceConfiguration : BaseEntityConfiguration<Price>
{
    public override void Configure(EntityTypeBuilder<Price> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.AssetClass, q.Symbol, q.Date}).IsUnique();
    }
}
