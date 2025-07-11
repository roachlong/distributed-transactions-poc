namespace DistributedTransactions.AllocationsApi.Models;

using DistributedTransactions.Domain.Allocations;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int Offset { get; set; }
    public int Count { get; set; }
    public DateTime AsOfSystemTime { get; set; }
    public DateTime Date { get; set; }
    public int PageSize { get; set; }
    public string? SearchSymbol { get; set; }
    public TradeDirection? Direction { get; set; }
    public TradeType? Type { get; set; }
    public TradeDestination? Destination { get; set; }
    public int? TotalRecords { get; set; } // only for initial request
    public int? TotalPages { get; set; } // only for initial request
    public List<TradeSearchPage>? Pages { get; set; } // only for initial request
}
