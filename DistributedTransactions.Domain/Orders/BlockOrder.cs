namespace DistributedTransactions.Domain.Orders;

public class BlockOrder : BaseOrderModel
{
    public required string Code { get; set; }
    public long Allocated { get; set; }
    public int Accounts { get; set; }
}
