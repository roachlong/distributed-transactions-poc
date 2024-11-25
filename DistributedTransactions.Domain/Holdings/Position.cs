namespace DistributedTransactions.Domain.Holdings;

public class Position : HoldingsDomainModel
{
    public required string AssetClass { get; set; }
    public required string Symbol { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public double Value { get; set; }
    public double Allocation { get; set; }
}
