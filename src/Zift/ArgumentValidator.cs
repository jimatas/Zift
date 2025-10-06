namespace Zift;

internal static class ArgumentValidator
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the value is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check for <see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter to include in the exception message.</param>
    /// <returns>The value if it is not <see langword="null"/>.</returns>
    public static T ThrowIfNull<T>(
        [NotNull] this T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);

        return value;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the string is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter to include in the exception message.</param>
    /// <returns>The string if it is not <see langword="null"/> or empty.</returns>
    public static string ThrowIfNullOrEmpty(
        [NotNull] this string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);

        return value;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the collection is <see langword="null"/>,
    /// or an <see cref="ArgumentException"/> if it is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="value">The collection to check.</param>
    /// <param name="paramName">The name of the parameter to include in the exception message.</param>
    /// <returns>The collection if it is not <see langword="null"/> or empty.</returns>
    public static IEnumerable<T> ThrowIfNullOrEmpty<T>(
        [NotNull] this IEnumerable<T>? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        value.ThrowIfNull(paramName);

        if (!value.Any())
        {
            throw new ArgumentException("Collection must contain at least one element.", paramName);
        }

        return value;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is less than the comparison value.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The value to compare against.</param>
    /// <param name="paramName">The name of the parameter to include in the exception message.</param>
    /// <returns>The value if it is not less than <paramref name="other"/>.</returns>
    public static T ThrowIfLessThan<T>(
        this T value,
        T other,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName);

        return value;
    }
}
