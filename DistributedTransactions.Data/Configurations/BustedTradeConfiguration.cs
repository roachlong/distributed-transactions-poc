using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class BustedTradeConfiguration : BaseTradeConfiguration<BustedTrade>
{
    public override void Configure(EntityTypeBuilder<BustedTrade> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.ActivityType)
            .HasDefaultValue(TradeActivityType.Bust);
    }
}
