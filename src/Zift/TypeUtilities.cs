namespace Zift;

internal static class TypeUtilities
{
    /// <summary>
    /// Returns a public instance property on the type with the given name, using case-insensitive matching.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <returns>The matching <see cref="PropertyInfo"/>, or <see langword="null"/> if none is found.</returns>
    public static PropertyInfo? GetPropertyIgnoreCase(this Type type, string propertyName)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        // Prioritize exact (case-sensitive) matches first.
        var caseSensitiveMatch = properties.FirstOrDefault(prop => string.Equals(prop.Name, propertyName, StringComparison.Ordinal));
        if (caseSensitiveMatch is not null)
        {
            return caseSensitiveMatch;
        }

        // Fallback to case-insensitive match if no exact match found.
        var caseInsensitiveMatch = properties.FirstOrDefault(prop => string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase));
        return caseInsensitiveMatch;
    }

    /// <summary>
    /// Determines whether the type is a nullable reference or value type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is nullable; otherwise, <see langword="false"/>.</returns>
    public static bool IsNullableType(this Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    /// <summary>
    /// Determines whether the type is a (non-string) collection.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a collection; otherwise, <see langword="false"/>.</returns>
    public static bool IsCollectionType(this Type type) =>
        type.GetInterfacesIncludingSelf().Contains(typeof(IEnumerable)) &&
        type != typeof(string);

    /// <summary>
    /// Gets the element type of a collection, if available.
    /// </summary>
    /// <param name="collectionType">The collection type to inspect.</param>
    /// <returns>The element type, or <see langword="null"/> if it cannot be determined.</returns>
    public static Type? GetCollectionElementType(this Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        return collectionType.GetInterfacesIncludingSelf()
            .FirstOrDefault(iface =>
                iface.IsGenericType &&
                iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))?
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
