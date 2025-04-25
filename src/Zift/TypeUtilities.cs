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
}
