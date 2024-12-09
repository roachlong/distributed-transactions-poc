using DistributedTransactions.Domain.Holdings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class PositionConfiguration : BaseEntityConfiguration<Position>
{
    public override void Configure(EntityTypeBuilder<Position> builder)
    {
        base.Configure(builder);
        builder.Property(e => e.Price)
            .HasColumnType("numeric(17, 2)");
        builder.Property(e => e.Value)
            .HasComputedColumnSql("\"Quantity\" * \"Price\"", stored: true)
            .HasColumnType("numeric(17, 2)");
        builder.Property(e => e.Allocation)
            .HasColumnType("numeric(5, 2)");
    }
}
