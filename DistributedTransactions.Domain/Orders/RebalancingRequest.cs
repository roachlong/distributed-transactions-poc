namespace DistributedTransactions.Domain.Orders;

public class RebalancingRequest : OrdersDomainModel
{
    public required long RequestNumber { get; set; }
    public required long GroupNumber { get; set; }
    public DateTime Date { get; set; }
}
