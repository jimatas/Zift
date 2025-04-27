namespace Zift;

internal static class ParameterNameGenerator
{
    public static string FromType(Type type)
    {
        var firstLetter = type.Name.FirstOrDefault(char.IsAsciiLetter);

        return firstLetter != default
            ? char.ToLowerInvariant(firstLetter).ToString()
            : "x";
    }

    public static string FromType<T>() => FromType(typeof(T));
}
