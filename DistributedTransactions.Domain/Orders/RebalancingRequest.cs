namespace DistributedTransactions.Domain.Orders;

public class RebalancingRequest : OrdersDomainModel
{
    public required int RequestNumber { get; set; }
    public required int GroupNumber { get; set; }
    public DateTime Date { get; set; }
}
