namespace DistributedTransactions.AllocationsApi.Models;

public class TradeSearchPage
{
    public int Page { get; set; }
    public int Offset { get; set; }
    public string? Cursor { get; set; }
}
