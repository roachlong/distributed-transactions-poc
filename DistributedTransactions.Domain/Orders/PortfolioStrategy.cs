namespace DistributedTransactions.Domain.Orders;

public enum PortfolioStrategy : int
{
    BuyAndHold,
    Diversification,
    DollarCostAveraging,
    InvestInGrowth,
    MarketTiming,
    Undefined
}
