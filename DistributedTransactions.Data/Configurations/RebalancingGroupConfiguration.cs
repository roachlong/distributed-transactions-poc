using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class RebalancingGroupConfiguration : BaseEntityConfiguration<RebalancingGroup>
{
    public override void Configure(EntityTypeBuilder<RebalancingGroup> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.GroupNumber}).IsUnique();
        builder.HasIndex(q => new {q.AssetClass, q.ManagerName, q.Strategy}).IsUnique();
    }
}
