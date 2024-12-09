using DistributedTransactions.Domain.Reference;
using Microsoft.EntityFrameworkCore;

namespace DistributedTransactions.Data;

public class ReferenceDbContext : BaseDbContext
{
    public required DbSet<Asset> Assets { get; set; }
    
    protected override string GetDatabaseName() {
        return "reference";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(ReferenceDomainModel);
    }
}
