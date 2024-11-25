using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public abstract class BaseTradeConfiguration<T> : BaseEntityConfiguration<T> where T : BaseTradeModel
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.BlockOrderCode, q.BlockOrderSeqNum, q.Date}).IsUnique();
    }
}
