namespace DistributedTransactions.Domain.Trades;

public enum TradeRestriction : int
{
    AllOrNone,
    Day,
    FillOrKill,
    GoodTilCancelled,
    ImmediateOrCancel,
    OnClose,
    OnOpen
}
