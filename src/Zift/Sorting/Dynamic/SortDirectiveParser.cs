﻿namespace Zift.Sorting.Dynamic;

/// <summary>
/// Default implementation of <see cref="ISortDirectiveParser{T}"/> using SQL-like syntax.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public class SortDirectiveParser<T> : ISortDirectiveParser<T>
{
    /// <inheritdoc/>
    public IEnumerable<ISortCriterion<T>> Parse(string directives)
    {
        directives.ThrowIfNullOrEmpty();

        var parts = directives.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Any(string.IsNullOrEmpty))
        {
            throw new FormatException("Invalid sorting string format: The string contains an empty sorting directive.");
        }

        return parts.Select(ParseDirective);
    }

    private static ISortCriterion<T> ParseDirective(string directive)
    {
        var parts = directive.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

        var property = parts[0];
        var direction = parts.Length switch
        {
            1 => SortDirection.Ascending,
            2 => ParseDirection(parts[1]),
            _ => throw new FormatException($"Invalid format for sorting directive: '{directive}'. Expected format is 'Property [ASC|DESC]'.")
        };

        try
        {
            return new SortCriterion<T>(property, direction);
        }
        catch (ArgumentException exception)
        {
            throw new FormatException(
                "Failed to parse a sorting directive from the input string. See the inner exception for details.",
                innerException: exception);
        }
    }

    private static SortDirection ParseDirection(string direction)
    {
        return direction.ToUpperInvariant() switch
        {
            "ASC" => SortDirection.Ascending,
            "DESC" => SortDirection.Descending,
            _ => throw new FormatException($"Invalid sort direction in sorting directive: '{direction}'. Must be either 'ASC' or 'DESC'.")
        };
    }
}
