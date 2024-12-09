namespace DistributedTransactions.Domain.Trades;

public enum TradeType : int
{
    Limit,
    Market,
    Stop,
    StopLimit,
    TrailingStop
}
