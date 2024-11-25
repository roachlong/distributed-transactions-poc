namespace DistributedTransactions.Domain.Trades;

public class ReplacedTrade : BaseTradeModel
{
    public TradeDestination? NewDestination { get; set; }
    public TradeType? NewType { get; set; }
    public TradeRestriction? NewRestriction { get; set; }
    public int? NewAmount { get; set; }
}
