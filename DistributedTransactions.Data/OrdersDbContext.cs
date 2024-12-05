using System.Data;
using DistributedTransactions.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace DistributedTransactions.Data;

public class OrdersDbContext : BaseDbContext
{
    public DbSet<RebalancingGroup> RebalancingGroups { get; set; }
    public DbSet<RebalancingRequest> RebalancingRequests { get; set; }
    public DbSet<RebalancingSecurity> RebalancingSecurities { get; set; }
    public DbSet<CustomerOrder> CustomerOrders { get; set; }
    public DbSet<BlockOrder> BlockOrders { get; set; }
    
    protected override string GetDatabaseName() {
        return "orders";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(OrdersDomainModel);
    }

    public int InsertRebalancingSecuritiesRawSql(RebalancingRequest request, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = $"""
                    INSERT INTO "RebalancingSecurities" (
                        "RequestNumber", "GroupNumber", "Date",
                        "AccountNumbers", "AssetClass", "Symbol"
                    ) 
                    SELECT r."RequestNumber", r."GroupNumber", r."Date", g."AccountNumbers",
                        g."AssetClass", unnest(g."SecuritySymbols") as "Symbol"
                    FROM "RebalancingGroups" g
                    JOIN "RebalancingRequests" r ON r."GroupNumber" = g."GroupNumber"
                    WHERE "RequestNumber" = {request.RequestNumber}
                    ON CONFLICT ("RequestNumber", "Symbol")
                    DO UPDATE SET
                        "GroupNumber" = excluded."GroupNumber",
                        "Date" = excluded."Date",
                        "AccountNumbers" = excluded."AccountNumbers",
                        "AssetClass" = excluded."AssetClass";
                    """;
                return Database.ExecuteSqlRaw(sql);
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing trades: {e.Message}");
                retries++;
            }
        }
        throw new DataException($"failed to insert trades after {retries} retries");
    }
}
