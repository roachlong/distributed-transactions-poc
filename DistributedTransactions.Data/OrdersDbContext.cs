using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace DistributedTransactions.Data;

public class OrdersDbContext : BaseDbContext
{
    public required DbSet<RebalancingGroup> RebalancingGroups { get; set; }
    public required DbSet<RebalancingRequest> RebalancingRequests { get; set; }
    public required DbSet<CustomerOrder> CustomerOrders { get; set; }
    public required DbSet<BlockOrder> BlockOrders { get; set; }
    
    protected override string GetDatabaseName() {
        return "orders";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(OrdersDomainModel);
    }
}
