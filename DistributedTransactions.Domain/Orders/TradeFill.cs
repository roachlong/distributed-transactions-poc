namespace DistributedTransactions.Domain.Orders;

public class TradeFill : OrdersDomainModel
{
    public required string BlockOrderCode { get; set; }
    public required int BlockOrderSeqNum { get; set; }
    public required DateTime Date { get; set; }
    public long? FilledQuantity { get; set; }
    public double? Price { get; set; }
    public long? CancelledQuantity { get; set; }
    public OrderDestination? NewDestination { get; set; }
    public OrderType? NewType { get; set; }
    public OrderRestriction? NewRestriction { get; set; }
    public long? NewQuantity { get; set; }
}
