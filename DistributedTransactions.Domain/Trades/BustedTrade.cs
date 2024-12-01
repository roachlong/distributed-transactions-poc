namespace DistributedTransactions.Domain.Trades;

public class BustedTrade : BaseTradeModel
{
    public int CancelledQuantity { get; set; }
}
