namespace Zift;

internal static class ArgumentValidator
{
    public static T ThrowIfNull<T>([NotNull] this T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);

        return value;
    }

    public static string ThrowIfNullOrEmpty([NotNull] this string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);

        return value;
    }

    public static IEnumerable<T> ThrowIfNullOrEmpty<T>([NotNull] this IEnumerable<T>? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        value.ThrowIfNull(paramName);

        if (!value.Any())
        {
            throw new ArgumentException("Collection must contain at least one element.", paramName);
        }

        return value;
    }

    public static T ThrowIfLessThan<T>(this T value, T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName);

        return value;
    }
}
