using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class RebalancingGroupConfiguration : BaseEntityConfiguration<RebalancingGroup>
{
    public override void Configure(EntityTypeBuilder<RebalancingGroup> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.GroupNumber)
            .HasDefaultValueSql("unordered_unique_rowid()");
        builder.HasIndex(q => new {q.GroupNumber}).IsUnique();
        builder.HasIndex(q => new {q.AssetClass, q.ManagerName, q.Strategy}).IsUnique();
    }
}
