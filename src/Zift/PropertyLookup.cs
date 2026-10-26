namespace Zift;

internal static class PropertyLookup
{
    public static PropertyInfo FindReadableProperty(
        this Type type,
        string name,
        Func<string, Exception?, Exception> createException)
    {
        PropertyInfo property;

        try
        {
            property = type.GetProperty(
                name,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? throw createException($"'{type.Name}' has no public property '{name}'.", null);
        }
        catch (AmbiguousMatchException exception)
        {
            throw createException($"Property '{name}' is ambiguous on type '{type.Name}'.", exception);
        }

        if (property.GetIndexParameters().Length > 0)
        {
            throw createException($"Indexed property '{name}' on type '{type.Name}' can't be used.", null);
        }

        if (property.GetMethod is not { IsPublic: true })
        {
            throw createException($"Property '{name}' on type '{type.Name}' can't be read.", null);
        }

        return property;
    }
}
