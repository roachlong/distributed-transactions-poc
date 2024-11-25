using DistributedTransactions.Domain.Market;
using Microsoft.EntityFrameworkCore;

namespace DistributedTransactions.Data;

public class MarketDbContext : BaseDbContext
{
    public required DbSet<Price> Prices { get; set; }
    
    protected override string GetDatabaseName() {
        return "market";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(MarketDomainModel);
    }
}
