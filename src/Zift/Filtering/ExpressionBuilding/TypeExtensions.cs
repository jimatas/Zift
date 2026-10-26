namespace Zift.Filtering.ExpressionBuilding;

internal static class TypeExtensions
{
    public static Type GetEffectiveType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    public static bool IsNumeric(this Type type)
    {
        return type == typeof(byte) ||
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
    }

    public static Type? GetCollectionElementType(this Type collectionType)
    {
        if (collectionType == typeof(string))
        {
            return null;
        }

        if (collectionType.IsArray)
        {
            return collectionType.IsSZArray
                ? collectionType.GetElementType()
                : null;
        }

        if (collectionType.IsGenericType &&
            collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return collectionType.GetGenericArguments()[0];
        }

        return collectionType
            .GetInterfaces()
            .FirstOrDefault(@interface =>
                @interface.IsGenericType &&
                @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))?
            .GetGenericArguments()[0];
    }
}
