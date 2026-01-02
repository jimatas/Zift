namespace Zift.Querying.ExpressionBuilding;

internal static class TypeExtensions
{
    public static bool IsNullable(this Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    public static Type GetEffectiveType(this Type type) =>
        Nullable.GetUnderlyingType(type) ?? type;

    public static bool IsNumeric(this Type type) =>
        type == typeof(byte) ||
        type == typeof(sbyte) ||
        type == typeof(short) ||
        type == typeof(ushort) ||
        type == typeof(int) ||
        type == typeof(uint) ||
        type == typeof(long) ||
        type == typeof(ulong) ||
        type == typeof(float) ||
        type == typeof(double) ||
        type == typeof(decimal);

    public static bool IsOrderable(this Type type) =>
        type != typeof(bool) && !type.IsEnum;

    public static Type? GetCollectionElementType(this Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        if (collectionType.IsGenericType &&
            collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return collectionType.GetGenericArguments()[0];
        }

        return collectionType
            .GetInterfaces()
            .FirstOrDefault(iface =>
                iface.IsGenericType &&
                iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))?
            .GetGenericArguments()[0];
    }
}
