namespace Zift;

internal static class ParameterNameGenerator
{
    /// <summary>
    /// Generates a default parameter name based on the given type.
    /// </summary>
    /// <param name="type">The type to generate a parameter name for.</param>
    /// <returns>A lowercase single-letter parameter name, or "x" if one cannot be determined.</returns>
    public static string FromType(Type type)
    {
        var firstLetter = type.Name.FirstOrDefault(char.IsAsciiLetter);

        return firstLetter != default
            ? char.ToLowerInvariant(firstLetter).ToString()
            : "x";
    }

    /// <summary>
    /// Generates a default parameter name based on the generic type argument.
    /// </summary>
    /// <typeparam name="T">The type to generate a parameter name for.</typeparam>
    /// <returns>A lowercase single-letter parameter name, or "x" if one cannot be determined.</returns>
    public static string FromType<T>() => FromType(typeof(T));
}
