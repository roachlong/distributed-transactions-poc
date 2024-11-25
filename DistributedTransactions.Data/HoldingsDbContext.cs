using DistributedTransactions.Domain.Holdings;
using Microsoft.EntityFrameworkCore;

namespace DistributedTransactions.Data;

public class HoldingsDbContext : BaseDbContext
{
    public required DbSet<Portfolio> Portfolios { get; set; }
    public required DbSet<Position> Positions { get; set; }
    
    protected override string GetDatabaseName() {
        return "holdings";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(HoldingsDomainModel);
    }
}
