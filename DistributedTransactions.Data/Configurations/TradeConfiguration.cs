using DistributedTransactions.Domain.Allocations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class TradeConfiguration : BaseEntityConfiguration<Trade>
{
    public override void Configure(EntityTypeBuilder<Trade> builder)
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
