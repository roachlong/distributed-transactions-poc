using DistributedTransactions.Domain.Allocations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class TradeConfiguration : BaseEntityConfiguration<Trade>
{
    public override void Configure(EntityTypeBuilder<Trade> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.BlockOrderCode, q.BlockOrderSeqNum, q.Date}).IsUnique();
    }
}
