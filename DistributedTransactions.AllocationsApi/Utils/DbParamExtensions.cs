namespace DistributedTransactions.AllocationsApi.Utils;

public static class DbParamExtensions
{
    public static object ToDbNullableEnum<T>(this T? enumValue) where T : struct, Enum =>
        enumValue.HasValue ? (object)Convert.ToInt32(enumValue.Value) : DBNull.Value;
}
