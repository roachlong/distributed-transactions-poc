using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class BlockOrderConfiguration : BaseOrderConfiguration<BlockOrder>
{
    public override void Configure(EntityTypeBuilder<BlockOrder> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.Code, q.Date}).IsUnique();
        builder.HasIndex(q => new {q.AssetClass, q.Symbol, q.Date, q.Direction,
                                   q.Destination, q.Type, q.Restriction}).IsUnique();
        builder.Property(p => p.Allocated)
            .HasDefaultValue(0);
    }
}
