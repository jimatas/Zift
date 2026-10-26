namespace Zift;

internal static class TypeExtensions
{
    public static bool IsNullable(this Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }
}
