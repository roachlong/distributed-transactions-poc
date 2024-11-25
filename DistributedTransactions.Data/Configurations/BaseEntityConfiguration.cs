using DistributedTransactions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistributedTransactions.Data.Configurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseDomainModel
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.CreatedOn)
            .HasDefaultValueSql("now()");
        builder.Property(p => p.CreatedBy)
            .HasDefaultValue("system");
        builder.Property(p => p.ModifiedOn)
            .HasDefaultValueSql("now()");
        builder.Property(p => p.ModifiedBy)
            .HasDefaultValue("system");
    }
}
