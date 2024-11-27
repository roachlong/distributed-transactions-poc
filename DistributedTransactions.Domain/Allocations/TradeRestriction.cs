namespace DistributedTransactions.Domain.Allocations;

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
