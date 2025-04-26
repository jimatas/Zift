namespace Zift;

internal static class TypeUtilities
{
    public static PropertyInfo? GetPropertyIgnoreCase(this Type type, string propertyName)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        return properties.FirstOrDefault(prop => string.Equals(prop.Name, propertyName, StringComparison.Ordinal))
            ?? properties.FirstOrDefault(prop => string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase));
    }
}
