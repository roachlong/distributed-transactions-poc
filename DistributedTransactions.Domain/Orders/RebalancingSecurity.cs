namespace DistributedTransactions.Domain.Orders;

public class RebalancingSecurity : OrdersDomainModel
{
    public required long RequestNumber { get; set; }
    public required long GroupNumber { get; set; }
    public DateTime Date { get; set; }
    public virtual List<string> AccountNumbers { get; set; } = new List<string>();
    public required string AssetClass { get; set; }
    public required string Symbol { get; set; }
}
