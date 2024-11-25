using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class AdHocTradeConfiguration : BaseTradeConfiguration<AdHocTrade>
{
    public override void Configure(EntityTypeBuilder<AdHocTrade> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.ActivityType)
            .HasDefaultValue(TradeActivityType.AdHoc);
    }
}
