using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class CustomerOrderConfiguration : BaseOrderConfiguration<CustomerOrder>
{
    public override void Configure(EntityTypeBuilder<CustomerOrder> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.RequestNumber, q.PositionId}).IsUnique();
    }
}
