namespace DistributedTransactions.Domain.Reference;

public class Asset : ReferenceDomainModel
{
    public required string AssetClass { get; set; }
    public required string Symbol { get; set; }
    public required string Name { get; set; }
    public double? MarketCap { get; set; }
    public string? Country { get; set; }
    public int? IPOYear { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
}
