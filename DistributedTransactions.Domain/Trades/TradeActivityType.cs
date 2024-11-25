namespace DistributedTransactions.Domain.Trades;

public enum TradeActivityType : int
{
    AdHoc,
    Bust,
    Execute,
    Replace
}
