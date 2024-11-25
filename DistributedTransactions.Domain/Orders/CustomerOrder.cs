namespace DistributedTransactions.Domain.Orders;

public class CustomerOrder : BaseOrderModel
{
    public required string AccountNum { get; set; }
    public Guid PositionId { get; set; }
}
