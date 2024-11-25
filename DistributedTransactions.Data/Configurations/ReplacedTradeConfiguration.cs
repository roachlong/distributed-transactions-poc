using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class ReplacedTradeConfiguration : BaseTradeConfiguration<ReplacedTrade>
{
    public override void Configure(EntityTypeBuilder<ReplacedTrade> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.ActivityType)
            .HasDefaultValue(TradeActivityType.Replace);
    }
}
