using System.Data;
using System.Globalization;
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

    public int InsertRebalancingSecurities(RebalancingRequest request, int maxRetries) {
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
                Console.WriteLine($"ERROR writing rebalancing securities: {e.Message}");
                retries++;
                Thread.Sleep(1000 * retries);
            }
        }
        throw new DataException($"failed to insert rebalancing securities after {retries} retries");
    }

    public List<OpeningPosition> GetOpeningPositions(RebalancingSecurity security, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = $"""
                    WITH portfolio AS (
                        SELECT p."Id", p."AccountNum",
                               CAST(p."Cash" AS FLOAT) + SUM(CAST(x."Quantity" AS FLOAT) * m."Open") AS "PortfolioValue"
                        FROM holdings."Portfolios" p
                        JOIN holdings."Positions" x ON x."PortfolioId" = p."Id"
                        JOIN market."Prices" m ON
                             m."AssetClass" = x."AssetClass"
                         AND m."Symbol" = x."Symbol"
                         AND CAST(m."Date" AS DATE) = '{security.Date.ToString("o", CultureInfo.InvariantCulture)}'
                        WHERE p."AccountNum" IN ('{string.Join("','", security.AccountNumbers)}')
                        GROUP BY p."Id", p."AccountNum"
                    )
                    SELECT p."AccountNum", x."Id" AS "PositionId", x."Quantity",
                           m."Open", p."PortfolioValue", x."Allocation"
                    FROM portfolio p
                    JOIN holdings."Positions" x ON
                         x."PortfolioId" = p."Id"
                     AND x."AssetClass" = '{security.AssetClass}'
                     AND x."Symbol" = '{security.Symbol}'
                    JOIN market."Prices" m ON
                         m."AssetClass" = x."AssetClass"
                     AND m."Symbol" = x."Symbol"
                     AND CAST(m."Date" AS DATE) = '{security.Date.ToString("o", CultureInfo.InvariantCulture)}';
                """;
                return [.. Database.SqlQueryRaw<OpeningPosition>(sql)];
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR reading opening positions: {e.Message}");
                retries++;
                Thread.Sleep(1000 * retries);
            }
        }
        throw new DataException($"failed to retrieve opening positions after {retries} retries");
    }

    /*
        Here we're uisng raw sql to create a multi-value insert for any number of records
        that can be processed in a single transaction (micro-batch).  This provides
        significant performance optimiazation, however, we have to pay attention to
        our object model field types and properly convert the values for the database.

        GUIDs require us to convert the value to a string
        Enums require us to convert the value to an int
        Dates require us to use proper ISO stirng formatting
        Nullable types require us to check for null values
        And make sure you put single quotes where they are required
    */
    public int InsertCustomerOrders(List<CustomerOrder> orders, int maxRetries) {
        var retries = 0;
        // with any transaction, no matter how unlikely, we should always expect
        // potential serializable isolation errors and be prepared to retry
        while (retries < maxRetries) {
            try {
                // setting up the sql for our multi-value insert for the provided number of records
                var sql = """
                    INSERT INTO "CustomerOrders" (
                        "RequestNumber", "AccountNum", "PositionId",
                        "AssetClass", "Symbol", "Date", "Direction",
                        "Destination", "Type", "Restriction", "Quantity"
                    ) VALUES 
                    """;
                
                // using list comprehension to create an array of records with the expected number of field values
                var values = from order in orders select string.Format(
                    @"({0}, '{1}', '{2}', '{3}', '{4}', '{5}', {6}, {7}, {8}, {9}, {10})",
                    order.RequestNumber, order.AccountNum,
                    order.PositionId.ToString(),
                    order.AssetClass, order.Symbol,
                    order.Date.ToString("o", CultureInfo.InvariantCulture),
                    (int) order.Direction, (int) order.Destination,
                    (int) order.Type, (int) order.Restriction, order.Quantity);
                
                // then join the value records together in a comma-separated string of tuples
                // and ignore duplicate records (conflicts) or we could also handle conflicts with the
                // ON CONFLICT DO UPDATE SET field1 = excluded.field1, field2 = excluded... syntax
                sql += String.Join(", ", values.ToArray()) + " ON CONFLICT DO NOTHING;";
                return Database.ExecuteSqlRaw(sql);
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing orders: {e.Message}");

                // in case of consecutive errors we'll progressively cool off before the next attempt
                retries++;
                Thread.Sleep(1000 * retries);
            }
        }
        throw new DataException($"failed to insert orders after {retries} retries");
    }

    public int CreateBlockOrder(RebalancingSecurity security, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = $"""
                    WITH listing AS (
                        SELECT "RequestNumber", "AssetClass", "Symbol", "Date",
                               "Direction", "Destination", "Type", "Restriction",
                               SUM("Quantity") AS "Quantity", COUNT("Id") AS "Accounts"
                        FROM "CustomerOrders"
                        WHERE "RequestNumber" = {security.RequestNumber}
                          AND "AssetClass" = '{security.AssetClass}'
                          AND "Symbol" = '{security.Symbol}'
                          AND CAST("Date" AS date) = '{security.Date.ToString("o", CultureInfo.InvariantCulture)}'
                        GROUP BY "RequestNumber", "AssetClass", "Symbol", "Date",
                                 "Direction", "Destination", "Type", "Restriction"
                    )
                    INSERT INTO "BlockOrders" (
                        "Code", "RequestNumber", "AssetClass", "Symbol", "Date", "Direction",
                        "Destination", "Type", "Restriction", "Quantity", "Accounts"
                    )
                    SELECT CHR(CAST(FLOOR(RANDOM() * 26 + 65) AS INT)) ||
                           CHR(CAST(FLOOR(RANDOM() * 26 + 65) AS INT)) ||
                           CHR(CAST(FLOOR(RANDOM() * 26 + 65) AS INT)) || '-' ||
                           LPAD(FLOOR(RANDOM() * 1000000)::TEXT, 6, '0') AS "Code",
                           listing.*
                    FROM listing
                    ON CONFLICT DO NOTHING;
                    """;
                return Database.ExecuteSqlRaw(sql);
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing block order: {e.Message}");
                retries++;
                Thread.Sleep(1000 * retries);
            }
        }
        throw new DataException($"failed to insert block order after {retries} retries");
    }
}

public class OpeningPosition {
    public string AccountNum { get; set; }
    public Guid PositionId { get; set; }
    public long Quantity { get; set; }
    public double Open { get; set; }
    public double PortfolioValue { get; set; }
    public double Allocation { get; set; }
}
