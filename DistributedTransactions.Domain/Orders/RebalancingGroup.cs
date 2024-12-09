namespace DistributedTransactions.Domain.Orders;

public class RebalancingGroup : OrdersDomainModel
{
    public required long GroupNumber { get; set; }
    public required string AssetClass { get; set; }
    public required string ManagerName { get; set; }
    public PortfolioStrategy Strategy { get; set; }
    public virtual List<string> AccountNumbers { get; set; } = new List<string>();
    public virtual List<string> SecuritySymbols { get; set; } = new List<string>();
}
