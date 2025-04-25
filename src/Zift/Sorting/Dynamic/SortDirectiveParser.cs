namespace Zift.Sorting.Dynamic;

public class SortDirectiveParser<T> : ISortDirectiveParser<T>
{
    public IEnumerable<ISortCriterion<T>> Parse(string sortString)
    {
        sortString.ThrowIfNullOrEmpty();

        var directives = sortString.Split(',', StringSplitOptions.TrimEntries);
        if (directives.Any(string.IsNullOrEmpty))
        {
            throw new FormatException("Invalid sorting string format: The string contains an empty sorting directive.");
        }

        return directives.Select(ParseDirective);
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

        return new SortCriterion<T>(property, direction);
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
