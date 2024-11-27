namespace DistributedTransactions.Domain.Allocations;

public enum TradeActivityType : int
{
    AdHoc,
    Bust,
    Execute,
    Replace
}
