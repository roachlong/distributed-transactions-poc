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
    public int Quantity { get; set; }
    public int Filled { get; set; }
    public int Cancelled { get; set; }
    public int Needed { get; set; }
    public double? Price { get; set; }
    public double? Amount { get; set; }
}
