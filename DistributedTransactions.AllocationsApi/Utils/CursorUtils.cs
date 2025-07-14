namespace DistributedTransactions.AllocationsApi.Utils;

using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;
using DistributedTransactions.AllocationsApi.Models;

public static class CursorUtils
{
    public static string Encode(TradeSearchCursor cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(json));
    }

    public static TradeSearchCursor Decode(string base64)
    {
        var json = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(base64));
        return JsonSerializer.Deserialize<TradeSearchCursor>(json)!;
    }
}