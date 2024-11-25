namespace DistributedTransactions.Domain.Orders;

public class BlockOrder : BaseOrderModel
{
    public required string Code { get; set; }
    public int Allocated { get; set; }
    public int Accounts { get; set; }
}
