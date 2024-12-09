namespace DistributedTransactions.Domain.Allocations;

public enum TradeType : int
{
    Limit,
    Market,
    Stop,
    StopLimit,
    TrailingStop
}
