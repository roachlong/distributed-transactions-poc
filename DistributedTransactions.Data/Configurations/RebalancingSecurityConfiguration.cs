using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public class RebalancingSecurityConfiguration : BaseEntityConfiguration<RebalancingSecurity>
{
    public override void Configure(EntityTypeBuilder<RebalancingSecurity> builder)
    {
        base.Configure(builder);
        builder.HasIndex(q => new {q.RequestNumber, q.Symbol}).IsUnique();
    }
}
