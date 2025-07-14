using System.Data;
using Npgsql;
using NpgsqlTypes;
using DistributedTransactions.AllocationsApi.Models;
using DistributedTransactions.AllocationsApi.Utils;
using DistributedTransactions.Domain.Allocations;

public class TradeRepository
{
    private readonly string _connectionString;

    public TradeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DateTime> GetFollowerReadTimestampAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT follower_read_timestamp()", conn);
        var result = await cmd.ExecuteScalarAsync();
        return (DateTime)result!;
    }

    public async Task<int> GetTotalCountAsync(
        DateTime asOf,
        DateTime date,
        string? symbol = null,
        TradeDirection? direction = null,
        TradeType? type = null,
        TradeDestination? destination = null)
    {
        string asOfLiteral = asOf.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff") + "Z";
        string sql = $@"
            SELECT COUNT(1)
            FROM ""Trades"" AS OF SYSTEM TIME '{asOfLiteral}'
            WHERE ""Date"" = @date
              AND (@symbol IS NULL OR ""Symbol"" = @symbol)
              AND (@direction IS NULL OR ""Direction"" = @direction)
              AND (@type IS NULL OR ""Type"" = @type OR ""NewType"" = @type)
              AND (@destination IS NULL OR ""Destination"" = @destination OR ""NewDestination"" = @destination)";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("date", date);
        cmd.Parameters.Add("symbol", NpgsqlDbType.Text).Value = (object?)symbol ?? DBNull.Value;
        cmd.Parameters.Add("direction", NpgsqlDbType.Integer).Value = direction.ToDbNullableEnum();
        cmd.Parameters.Add("type", NpgsqlDbType.Integer).Value = type.ToDbNullableEnum();
        cmd.Parameters.Add("destination", NpgsqlDbType.Integer).Value = destination.ToDbNullableEnum();

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<TradeSearchPage>> GetPageCursorsAsync(
        DateTime asOf,
        DateTime date,
        int pageSize,
        string? symbol = null,
        TradeDirection? direction = null,
        TradeType? type = null,
        TradeDestination? destination = null)
    {
        var pages = new List<TradeSearchPage>();

        string asOfLiteral = asOf.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff") + "Z";
        string sql = $@"
            SELECT t.""Symbol"", COALESCE(t.""Quantity"", 0), t.""Id"", t.rn - 1, floor((t.rn - 1) / @pageSize) + 1 AS page_num
            FROM (
                SELECT ""Symbol"", ""Quantity"", ""Id"",
                       row_number() OVER (ORDER BY ""Symbol"" ASC, ""Quantity"" DESC, ""Id"" ASC) AS rn
                FROM ""Trades""
                WHERE ""Date"" = @date
                  AND (@symbol IS NULL OR ""Symbol"" = @symbol)
                  AND (@direction IS NULL OR ""Direction"" = @direction)
                  AND (@type IS NULL OR ""Type"" = @type OR ""NewType"" = @type)
                  AND (@destination IS NULL OR ""Destination"" = @destination OR ""NewDestination"" = @destination)
            ) t AS OF SYSTEM TIME '{asOfLiteral}'
            WHERE t.rn % @pageSize = 1
            ORDER BY t.rn";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("date", date);
        cmd.Parameters.AddWithValue("pageSize", pageSize);
        cmd.Parameters.Add("symbol", NpgsqlDbType.Text).Value = (object?)symbol ?? DBNull.Value;
        cmd.Parameters.Add("direction", NpgsqlDbType.Integer).Value = direction.ToDbNullableEnum();
        cmd.Parameters.Add("type", NpgsqlDbType.Integer).Value = type.ToDbNullableEnum();
        cmd.Parameters.Add("destination", NpgsqlDbType.Integer).Value = destination.ToDbNullableEnum();

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var cursor = new TradeSearchCursor
            {
                Symbol = reader.GetString(0),
                Quantity = reader.GetInt64(1),
                Id = reader.GetGuid(2),
                Offset = reader.GetInt32(3),
                Page = reader.GetInt32(4),
                AsOfSystemTime = asOf,
                Date = date,
                PageSize = pageSize,
                SearchSymbol = symbol,
                Direction = direction,
                Type = type,
                Destination = destination
            };

            pages.Add(new TradeSearchPage
            {
                Offset = cursor.Offset,
                Page = cursor.Page,
                Cursor = CursorUtils.Encode(cursor)
            });
        }

        return pages;
    }

    public async Task<List<Trade>> GetTradesPageAsync(TradeSearchCursor cursor)
    {
        var trades = new List<Trade>();

        string asOfLiteral = cursor.AsOfSystemTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.ffffff") + "Z";
        string sql = $@"
            SELECT *
            FROM ""Trades"" AS OF SYSTEM TIME '{asOfLiteral}'
            WHERE ""Date"" = @date
            AND (@symbol IS NULL OR ""Symbol"" = @symbol)
            AND (@direction IS NULL OR ""Direction"" = @direction)
            AND (@type IS NULL OR ""Type"" = @type OR ""NewType"" = @type)
            AND (@destination IS NULL OR ""Destination"" = @destination OR ""NewDestination"" = @destination)
            AND (
                (""Symbol"" > @afterSymbol)
                OR (
                ""Symbol"" = @afterSymbol AND (
                    COALESCE(""Quantity"", 0) < @afterQuantity
                    OR (COALESCE(""Quantity"", 0) = @afterQuantity AND ""Id"" > @afterId)
                )
                )
            )
            ORDER BY ""Symbol"" ASC, ""Quantity"" DESC, ""Id"" ASC
            LIMIT @pageSize";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("date", cursor.Date);
        cmd.Parameters.AddWithValue("pageSize", cursor.PageSize);
        cmd.Parameters.AddWithValue("afterSymbol", cursor.Symbol);
        cmd.Parameters.AddWithValue("afterQuantity", cursor.Quantity);
        cmd.Parameters.AddWithValue("afterId", cursor.Id);
        cmd.Parameters.Add("symbol", NpgsqlDbType.Text).Value = (object?)cursor.SearchSymbol ?? DBNull.Value;
        cmd.Parameters.Add("direction", NpgsqlDbType.Integer).Value = cursor.Direction.ToDbNullableEnum();
        cmd.Parameters.Add("type", NpgsqlDbType.Integer).Value = cursor.Type.ToDbNullableEnum();
        cmd.Parameters.Add("destination", NpgsqlDbType.Integer).Value = cursor.Destination.ToDbNullableEnum();

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            trades.Add(new Trade
            {
                ActivityType = (TradeActivityType)reader.GetInt32(reader.GetOrdinal("ActivityType")),
                BlockOrderCode = reader.GetString(reader.GetOrdinal("BlockOrderCode")),
                BlockOrderSeqNum = reader.GetInt32(reader.GetOrdinal("BlockOrderSeqNum")),
                AssetClass = reader.GetString(reader.GetOrdinal("AssetClass")),
                Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                Direction = (TradeDirection)reader.GetInt32(reader.GetOrdinal("Direction")),
                Destination = (TradeDestination)reader.GetInt32(reader.GetOrdinal("Destination")),
                Type = (TradeType)reader.GetInt32(reader.GetOrdinal("Type")),
                Restriction = (TradeRestriction)reader.GetInt32(reader.GetOrdinal("Restriction")),
                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? null : reader.GetInt64(reader.GetOrdinal("Quantity")),
                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? null : (double?)reader.GetDecimal(reader.GetOrdinal("Price")),
                Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? null : (double?)reader.GetDecimal(reader.GetOrdinal("Amount")),
                CancelledQuantity = reader.IsDBNull(reader.GetOrdinal("CancelledQuantity")) ? null : reader.GetInt64(reader.GetOrdinal("CancelledQuantity")),
                NewDestination = reader.IsDBNull(reader.GetOrdinal("NewDestination")) ? null : (TradeDestination?)reader.GetInt32(reader.GetOrdinal("NewDestination")),
                NewType = reader.IsDBNull(reader.GetOrdinal("NewType")) ? null : (TradeType?)reader.GetInt32(reader.GetOrdinal("NewType")),
                NewRestriction = reader.IsDBNull(reader.GetOrdinal("NewRestriction")) ? null : (TradeRestriction?)reader.GetInt32(reader.GetOrdinal("NewRestriction")),
                NewQuantity = reader.IsDBNull(reader.GetOrdinal("NewQuantity")) ? null : reader.GetInt64(reader.GetOrdinal("NewQuantity"))
            });
        }

        return trades;
    }
}
