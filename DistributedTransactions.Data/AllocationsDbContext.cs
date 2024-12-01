using System.Data;
using System.Globalization;
using DistributedTransactions.Domain.Allocations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace DistributedTransactions.Data;

public class AllocationsDbContext : BaseDbContext
{
    public DbSet<Trade> Trades { get; set; }
    
    protected override string GetDatabaseName() {
        return "allocations";
    }
    
    protected override Type GetDomainModelType() {
        return typeof(AllocationsDomainModel);
    }

    public int InsertTradesRawSql(List<Trade> trades, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                var sql = """
                    INSERT INTO "Trades" (
                        "Id", "ActivityType", "BlockOrderCode", "BlockOrderSeqNum",
                        "AssetClass", "Symbol", "Date", "Direction", "Destination",
                        "Type", "Restriction", "Quantity", "Price",
                        "CancelledQuantity", "NewDestination", "NewType", "NewRestriction",
                        "NewQuantity", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn"
                    ) VALUES 
                    """;
                var values = from trade in trades select string.Format(
                    @"('{0}', {1}, '{2}', {3}, '{4}', '{5}', '{6}', {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, '{18}', '{19}', '{20}', '{21}')",
                    trade.Id.ToString(), (int) trade.ActivityType, trade.BlockOrderCode,
                    trade.BlockOrderSeqNum, trade.AssetClass, trade.Symbol,
                    trade.Date.ToString("o", CultureInfo.InvariantCulture),
                    (int) trade.Direction, (int) trade.Destination, (int) trade.Type,
                    (int) trade.Restriction, trade.Quantity == null ? "null" : trade.Quantity,
                    trade.Price == null ? "null" : trade.Price,
                    trade.CancelledQuantity == null ? "null" : trade.CancelledQuantity,
                    (int?) trade.NewDestination == null ? "null" : (int?) trade.NewDestination,
                    (int?) trade.NewType == null ? "null" : (int?) trade.NewType,
                    (int?) trade.NewRestriction == null ? "null" : (int?) trade.NewRestriction,
                    trade.NewQuantity == null ? "null" : trade.NewQuantity,
                    trade.CreatedBy, trade.CreatedOn.ToString("o", CultureInfo.InvariantCulture),
                    trade.ModifiedBy, trade.ModifiedOn.ToString("o", CultureInfo.InvariantCulture));
                sql += String.Join(", ", values.ToArray()) + " ON CONFLICT DO NOTHING;";
                return Database.ExecuteSqlRaw(sql);
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing trades: {e.Message}");
                retries++;
            }
        }
        throw new DataException($"failed to insert trades after {retries} retries");
    }

    public int InsertTradesDbCommand(List<Trade> trades, int maxRetries) {
        var retries = 0;
        while (retries < maxRetries) {
            try {
                using NpgsqlConnection conn = (NpgsqlConnection) Database.GetDbConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                var sql = """
                    INSERT INTO "Trades" (
                        "Id", "ActivityType", "BlockOrderCode", "BlockOrderSeqNum",
                        "AssetClass", "Symbol", "Date", "Direction", "Destination",
                        "Type", "Restriction", "Quantity", "Price",
                        "CancelledQuantity", "NewDestination", "NewType", "NewRestriction",
                        "NewQuantity", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn"
                    ) VALUES
                    """;
                var p = 0;
                var paramList = new object[trades.Count * 22];
                foreach (var trade in trades)
                {
                    var v = p;
                    sql += string.Format(@" (
                        @p{0}, @p{1}, @p{2}, @p{3}, @p{4}, @p{5},
                        @p{6}, @p{7}, @p{8}, @p{9}, @p{10}, @p{11},
                        @p{12}, @p{13}, @p{14}, @p{15}, @p{16},
                        @p{17}, @p{18}, @p{19}, @p{20}, @p{21}
                        ),",
                        p++, p++, p++, p++, p++, p++, p++, p++,
                        p++, p++, p++, p++, p++, p++, p++, p++,
                        p++, p++, p++, p++, p++, p++
                    );

                    paramList[v] = CreateParameter(cmd, $"@p{v}", NpgsqlDbType.Uuid, trade.Id);
                    paramList[v + 1] = CreateParameter(cmd, $"@p{v + 1}", NpgsqlDbType.Smallint, (int) trade.ActivityType);
                    paramList[v + 2] = CreateParameter(cmd, $"@p{v + 2}", NpgsqlDbType.Varchar, trade.BlockOrderCode);
                    paramList[v + 3] = CreateParameter(cmd, $"@p{v + 3}", NpgsqlDbType.Smallint, trade.BlockOrderSeqNum);
                    paramList[v + 4] = CreateParameter(cmd, $"@p{v + 4}", NpgsqlDbType.Varchar, trade.AssetClass);
                    paramList[v + 5] = CreateParameter(cmd, $"@p{v + 5}", NpgsqlDbType.Varchar, trade.Symbol);
                    paramList[v + 6] = CreateParameter(cmd, $"@p{v + 6}", NpgsqlDbType.TimestampTz, trade.Date);
                    paramList[v + 7] = CreateParameter(cmd, $"@p{v + 7}", NpgsqlDbType.Smallint, (int) trade.Direction);
                    paramList[v + 8] = CreateParameter(cmd, $"@p{v + 8}", NpgsqlDbType.Smallint, (int) trade.Destination);
                    paramList[v + 9] = CreateParameter(cmd, $"@p{v + 9}", NpgsqlDbType.Smallint, (int) trade.Type);
                    paramList[v + 10] = CreateParameter(cmd, $"@p{v + 10}", NpgsqlDbType.Smallint, (int) trade.Restriction);
                    paramList[v + 11] = CreateParameter(cmd, $"@p{v + 11}", NpgsqlDbType.Smallint, trade.Quantity);
                    paramList[v + 12] = CreateParameter(cmd, $"@p{v + 12}", NpgsqlDbType.Numeric, trade.Price);
                    paramList[v + 15] = CreateParameter(cmd, $"@p{v + 13}", NpgsqlDbType.Integer, trade.CancelledQuantity);
                    paramList[v + 16] = CreateParameter(cmd, $"@p{v + 14}", NpgsqlDbType.Smallint, (int?) trade.NewDestination);
                    paramList[v + 17] = CreateParameter(cmd, $"@p{v + 15}", NpgsqlDbType.Smallint, (int?) trade.NewType);
                    paramList[v + 18] = CreateParameter(cmd, $"@p{v + 16}", NpgsqlDbType.Smallint, (int?) trade.NewRestriction);
                    paramList[v + 19] = CreateParameter(cmd, $"@p{v + 17}", NpgsqlDbType.Integer, trade.NewQuantity);
                    paramList[v + 20] = CreateParameter(cmd, $"@p{v + 18}", NpgsqlDbType.Varchar, trade.CreatedBy);
                    paramList[v + 21] = CreateParameter(cmd, $"@p{v + 19}", NpgsqlDbType.TimestampTz, trade.CreatedOn);
                    paramList[v + 22] = CreateParameter(cmd, $"@p{v + 20}", NpgsqlDbType.Varchar, trade.ModifiedBy);
                    paramList[v + 23] = CreateParameter(cmd, $"@p{v + 21}", NpgsqlDbType.TimestampTz, trade.ModifiedOn);
                }
                cmd.CommandText = sql.Remove(sql.Length - 1, 1) + " ON CONFLICT DO NOTHING;";
                cmd.Parameters.AddRange(paramList);
                return cmd.ExecuteNonQuery();
            }
            catch (DataException e) {
                Console.WriteLine($"ERROR writing trades: {e.Message}");
                retries++;
            }
        }
        throw new DataException($"failed to insert trades after {retries} retries");
    }

    private static NpgsqlParameter CreateParameter(
        NpgsqlCommand cmd, String name, NpgsqlDbType type, object value
    ) {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.NpgsqlDbType = type;
        if (value == null) {
            param.Value = DBNull.Value;
        }
        else {
            param.Value = value;
        }
        return param;
    }
}
