namespace DistributedTransactions.Domain.Trades;

public class BustedTrade : BaseTradeModel
{
    public long CancelledQuantity { get; set; }
}
