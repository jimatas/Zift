namespace Zift;

internal static class TypeUtilities
{
    public static string GenerateParameterName(this Type type)
    {
        var firstLetter = type.Name.FirstOrDefault(char.IsAsciiLetter);

        return firstLetter != default
            ? char.ToLowerInvariant(firstLetter).ToString()
            : "x";
    }

    public static PropertyInfo? GetPropertyIgnoreCase(this Type type, string propertyName)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        return properties.FirstOrDefault(prop => string.Equals(prop.Name, propertyName, StringComparison.Ordinal))
            ?? properties.FirstOrDefault(prop => string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsNullableType(this Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    public static bool IsCollectionType(this Type type)
    {
        return type.GetInterfacesIncludingSelf().Contains(typeof(IEnumerable)) && type != typeof(string);
    }

    public static Type? GetCollectionElementType(this Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        return collectionType.GetInterfacesIncludingSelf()
            .FirstOrDefault(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))?
            .GetGenericArguments().Single();
    }

    private static List<Type> GetInterfacesIncludingSelf(this Type type)
    {
        var interfaces = type.GetInterfaces().ToList();
        if (type.IsInterface && !interfaces.Contains(type))
        {
            interfaces.Insert(0, type);
        }

        return interfaces;
    }
}
