namespace DistributedTransactions.Domain.Orders;

public abstract class BaseOrderModel : OrdersDomainModel
{
    public required long RequestNumber { get; set; }
    public required string AssetClass { get; set; }
    public required string Symbol { get; set; }
    public DateTime Date { get; set; }
    public OrderDirection Direction { get; set; }
    public OrderDestination Destination { get; set; }
    public OrderType Type { get; set; }
    public OrderRestriction Restriction { get; set; }
    public long Quantity { get; set; }
    public long Filled { get; set; }
    public long Cancelled { get; set; }
    public long Needed { get; set; }
    public double? Price { get; set; }
    public double? Amount { get; set; }
}
