using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class TradeFillConfiguration : BaseEntityConfiguration<TradeFill>
{
    public override void Configure(EntityTypeBuilder<TradeFill> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.BlockOrderCode, q.BlockOrderSeqNum, q.Date}).IsUnique();
        builder.Property(e => e.Price)
            .HasColumnType("numeric(17, 2)");
    }
}
