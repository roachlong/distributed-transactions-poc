namespace DistributedTransactions.AllocationsApi.Models;

using DistributedTransactions.Domain.Allocations;

public class TradeSearchCursor
{
    // Pagination anchor
    public string Symbol { get; set; } = default!;
    public long Quantity { get; set; }
    public Guid Id { get; set; }
    public int Page { get; set; }
    public int Offset { get; set; }

    // Immutable search context
    public DateTime AsOfSystemTime { get; set; }
    public DateTime Date { get; set; }
    public int PageSize { get; set; }
    public string? SearchSymbol { get; set; }
    public TradeDirection? Direction { get; set; }
    public TradeType? Type { get; set; }
    public TradeDestination? Destination { get; set; }
}
