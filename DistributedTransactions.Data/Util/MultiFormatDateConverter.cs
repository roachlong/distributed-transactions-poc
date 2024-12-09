using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DistributedTransactions.Data.Util;

public class MultiFormatDateConverter : JsonConverter<DateTime>
{
    public List<String> DateTimeFormats { get; set; }

    public override DateTime Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options
    ) {
        string dateString = reader.GetString()!;
        if (dateString == null) {
            throw new JsonException("Unable to parse null as a date.");
        }

        DateTime date;
        foreach (string format in DateTimeFormats) {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out date)) {
                return date;
            }
        }

        throw new JsonException("Unable to parse \"" + dateString + "\" as a date.");
    }

    public override void Write(
        Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options
    ) {
        throw new NotImplementedException();
    }
}