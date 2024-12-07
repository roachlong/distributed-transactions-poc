using System.Data;
using System.Globalization;
using DistributedTransactions.Domain.Orders;
using DistributedTransactions.Domain.Trades;
using Microsoft.EntityFrameworkCore;

namespace DistributedTransactions.Data;

public class TradesDbContext : BaseDbContext
{
    public DbSet<BustedTrade> BustedTrades { get; set; }
    public DbSet<ExecutedTrade> ExecutedTrades { get; set; }
    public DbSet<ReplacedTrade> ReplacedTrades { get; set; }
    
    protected override string GetDatabaseName() {
        return "trades";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(TradesDomainModel);
    }

    public TradeOrder GetTradingPriceRange(BlockOrder order, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = $"""
                    SELECT '{order.Code}' AS "Code",
                           '{order.AssetClass}' AS "AssetClass", '{order.Symbol}' AS "Symbol",
                           CAST('{order.Date.ToString("o", CultureInfo.InvariantCulture)}' AS DATE) AS "Date",
                           {(int) order.Direction} AS "Direction", {(int) order.Destination} AS "Destination",
                           {(int) order.Type} AS "Type", {(int) order.Restriction} AS "Restriction",
                           {order.Quantity} AS "Quantity", {order.Needed} AS "Needed", "High", "Low"
                    FROM market."Prices"
                    WHERE "AssetClass" = '{order.AssetClass}'
                      AND "Symbol" = '{order.Symbol}'
                      AND CAST("Date" AS DATE) = '{order.Date.ToString("o", CultureInfo.InvariantCulture)}'
                """;
                return Database.SqlQueryRaw<TradeOrder>(sql).OrderByDescending(r => r.Date).FirstOrDefault();
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR reading daily price range: {e.Message}");
                retries++;
            }
        }
        throw new DataException($"failed to retrieve daily price range after {retries} retries");
    }

    public int InsertReplacedTrades(List<ReplacedTrade> trades, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = """
                    INSERT INTO "ReplacedTrades" (
                        "BlockOrderCode", "BlockOrderSeqNum", "AssetClass",
                        "Symbol", "Date", "Direction", "Destination",
                        "Type", "Restriction", "NewDestination",
                        "NewType", "NewRestriction", "NewQuantity"
                    ) VALUES 
                    """;
                var values = from trade in trades select string.Format(
                    @"('{0}', {1}, '{2}', '{3}', '{4}', {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
                    trade.BlockOrderCode, trade.BlockOrderSeqNum, trade.AssetClass, trade.Symbol,
                    trade.Date.ToString("o", CultureInfo.InvariantCulture),
                    (int) trade.Direction, (int) trade.Destination,
                    (int) trade.Type, (int) trade.Restriction,
                    (int?) trade.NewDestination == null ? "null" : (int?) trade.NewDestination,
                    (int?) trade.NewType == null ? "null" : (int?) trade.NewType,
                    (int?) trade.NewRestriction == null ? "null" : (int?) trade.NewRestriction,
                    trade.NewQuantity == null ? "null" : trade.NewQuantity);
                sql += String.Join(", ", values.ToArray()) + " ON CONFLICT DO NOTHING;";
                return Database.ExecuteSqlRaw(sql);
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing amended trades: {e.Message}");
                retries++;
            }
        }
        throw new DataException($"failed to insert amended trades after {retries} retries");
    }

    public int InsertBustedTrades(List<BustedTrade> trades, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = """
                    INSERT INTO "BustedTrades" (
                        "BlockOrderCode", "BlockOrderSeqNum", "AssetClass",
                        "Symbol", "Date", "Direction", "Destination",
                        "Type", "Restriction", "CancelledQuantity"
                    ) VALUES 
                    """;
                var values = from trade in trades select string.Format(
                    @"('{0}', {1}, '{2}', '{3}', '{4}', {5}, {6}, {7}, {8}, {9})",
                    trade.BlockOrderCode, trade.BlockOrderSeqNum, trade.AssetClass, trade.Symbol,
                    trade.Date.ToString("o", CultureInfo.InvariantCulture),
                    (int) trade.Direction, (int) trade.Destination,
                    (int) trade.Type, (int) trade.Restriction, trade.CancelledQuantity);
                sql += String.Join(", ", values.ToArray()) + " ON CONFLICT DO NOTHING;";
                return Database.ExecuteSqlRaw(sql);
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing cancelled trades: {e.Message}");
                retries++;
            }
        }
        throw new DataException($"failed to insert cancelled trades after {retries} retries");
    }

    public int InsertExecutedTrades(List<ExecutedTrade> trades, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = """
                    INSERT INTO "ExecutedTrades" (
                        "BlockOrderCode", "BlockOrderSeqNum", "AssetClass",
                        "Symbol", "Date", "Direction", "Destination",
                        "Type", "Restriction", "Quantity", "Price"
                    ) VALUES 
                    """;
                var values = from trade in trades select string.Format(
                    @"('{0}', {1}, '{2}', '{3}', '{4}', {5}, {6}, {7}, {8}, {9}, {10})",
                    trade.BlockOrderCode, trade.BlockOrderSeqNum, trade.AssetClass, trade.Symbol,
                    trade.Date.ToString("o", CultureInfo.InvariantCulture),
                    (int) trade.Direction, (int) trade.Destination,
                    (int) trade.Type, (int) trade.Restriction, trade.Quantity, trade.Price);
                sql += String.Join(", ", values.ToArray()) + " ON CONFLICT DO NOTHING;";
                return Database.ExecuteSqlRaw(sql);
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing executed trades: {e.Message}");
                retries++;
            }
        }
        throw new DataException($"failed to insert executed trades after {retries} retries");
    }
}

public class TradeOrder {
    public string Code { get; set; }
    public string AssetClass { get; set; }
    public string Symbol { get; set; }
    public DateTime Date { get; set; }
    public OrderDirection Direction { get; set; }
    public OrderDestination Destination { get; set; }
    public OrderType Type { get; set; }
    public OrderRestriction Restriction { get; set; }
    public int Quantity { get; set; }
    public int Needed { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
}
