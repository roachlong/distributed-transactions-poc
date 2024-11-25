namespace DistributedTransactions.Domain.Trades;

public class AdHocTrade : BaseTradeModel
{
    public required string AccountNum { get; set; }
    public Guid PositionId { get; set; }
}
