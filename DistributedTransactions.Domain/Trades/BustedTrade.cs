namespace DistributedTransactions.Domain.Trades;

public class BustedTrade : BaseTradeModel
{
    public int CancelledAmount { get; set; }
}
