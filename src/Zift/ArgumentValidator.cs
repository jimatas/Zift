namespace Zift;

internal static class ArgumentValidator
{
    public static T ThrowIfNull<T>(this T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);

        return value;
    }

    public static string ThrowIfNullOrEmpty(this string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);

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
