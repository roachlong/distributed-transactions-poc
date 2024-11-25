using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public abstract class BaseOrderConfiguration<T> : BaseEntityConfiguration<T> where T : BaseOrderModel
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.Filled)
            .HasDefaultValue(0);
        builder.Property(e => e.Needed)
            .HasComputedColumnSql("\"Amount\" - \"Filled\"", stored: true);
    }
}
