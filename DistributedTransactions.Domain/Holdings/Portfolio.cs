namespace DistributedTransactions.Domain.Holdings;

public class Portfolio : HoldingsDomainModel
{
    public required string AccountNum { get; set; }
    public required PortfolioManager Manager { get; set; }
    public DateTime OpenedOn { get; set; }
    public double Cash { get; set; }
    public PortfolioStrategy Strategy { get; set; }
    public virtual List<Position> Positions { get; set; } = new List<Position>();
}
