namespace DistributedTransactions.Domain.Orders;

public enum OrderRestriction : int
{
    AllOrNone,
    Day,
    FillOrKill,
    GoodTilCancelled,
    ImmediateOrCancel,
    OnClose,
    OnOpen
}
