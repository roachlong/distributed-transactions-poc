using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class RebalancingRequestConfiguration : BaseEntityConfiguration<RebalancingRequest>
{
    public override void Configure(EntityTypeBuilder<RebalancingRequest> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.RequestNumber)
            .HasDefaultValueSql("unordered_unique_rowid()");
        builder.HasIndex(q => new {q.RequestNumber}).IsUnique();
        builder.HasIndex(q => new {q.GroupNumber, q.Date}).IsUnique();
    }
}
