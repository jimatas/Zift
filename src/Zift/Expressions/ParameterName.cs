namespace Zift.Expressions;

internal static class ParameterName
{
    public const string Default = "x";

    public static string FromType<T>() => FromType(typeof(T));
    public static string FromType(Type type)
    {
        var firstLetter = type.Name.FirstOrDefault(char.IsAsciiLetter);

        return firstLetter != default
            ? char.ToLowerInvariant(firstLetter).ToString()
            : Default;
    }
}
