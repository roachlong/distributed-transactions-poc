namespace DistributedTransactions.Domain.Market;

public class Price : MarketDomainModel
{
    public required string AssetClass { get; set; }
    public required string Symbol { get; set; }
    public DateTime Date { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public int Volume { get; set; }
}
