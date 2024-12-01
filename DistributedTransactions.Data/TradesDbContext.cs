using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore;

namespace DistributedTransactions.Data;

public class TradesDbContext : BaseDbContext
{
    public required DbSet<BustedTrade> BustedTrades { get; set; }
    public required DbSet<ExecutedTrade> ExecutedTrades { get; set; }
    public required DbSet<ReplacedTrade> ReplacedTrades { get; set; }
    
    protected override string GetDatabaseName() {
        return "trades";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(TradesDomainModel);
    }
}
