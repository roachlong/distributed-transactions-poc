namespace DistributedTransactions.Domain.Allocations;

public class Trade : AllocationsDomainModel
{
    public TradeActivityType ActivityType { get; set; }
    public required string BlockOrderCode { get; set; }
    public int BlockOrderSeqNum { get; set; }
    public required string AssetClass { get; set; }
    public required string Symbol { get; set; }
    public DateTime Date { get; set; }
    public TradeDirection Direction { get; set; }
    public TradeDestination Destination { get; set; }
    public TradeType Type { get; set; }
    public TradeRestriction Restriction { get; set; }
    public int? Amount { get; set; }
    public double? Price { get; set; }
    public string? AccountNum { get; set; }
    public Guid? PositionId { get; set; }
    public int? CancelledAmount { get; set; }
    public TradeDestination? NewDestination { get; set; }
    public TradeType? NewType { get; set; }
    public TradeRestriction? NewRestriction { get; set; }
    public int? NewAmount { get; set; }
}
