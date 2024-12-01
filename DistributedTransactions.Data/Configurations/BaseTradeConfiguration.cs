using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public abstract class BaseTradeConfiguration<T> : BaseEntityConfiguration<T> where T : BaseTradeModel
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(e => e.Price)
            .HasColumnType("numeric(17, 2)");
        builder.Property(e => e.Amount)
            .HasColumnType("numeric(17, 2)");
        builder.Property(e => e.Amount)
            .HasComputedColumnSql("\"Quantity\" * \"Price\"", stored: true);
        builder.HasIndex(q => new {q.BlockOrderCode, q.BlockOrderSeqNum, q.Date}).IsUnique();
    }
}
