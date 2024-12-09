using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class ExecutedTradeConfiguration : BaseTradeConfiguration<ExecutedTrade>
{
    public override void Configure(EntityTypeBuilder<ExecutedTrade> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.ActivityType)
            .HasDefaultValue(TradeActivityType.Execute);
    }
}
