namespace DistributedTransactions.Domain.Orders;

public enum OrderType : int
{
    Limit,
    Market,
    Stop,
    StopLimit,
    TrailingStop
}
