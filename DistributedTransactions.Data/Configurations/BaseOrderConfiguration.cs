using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public abstract class BaseOrderConfiguration<T> : BaseEntityConfiguration<T> where T : BaseOrderModel
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(e => e.Price)
            .HasColumnType("numeric(17, 2)");
        builder.Property(e => e.Amount)
            .HasColumnType("numeric(17, 2)");
        builder.Property(p => p.Filled)
            .HasDefaultValue(0);
        builder.Property(p => p.Cancelled)
            .HasDefaultValue(0);
        builder.Property(e => e.Needed)
            .HasComputedColumnSql("\"Quantity\" - \"Filled\" - \"Cancelled\"", stored: true);
        builder.Property(e => e.Amount)
            .HasComputedColumnSql("\"Filled\" * \"Price\"", stored: true);
    }
}
