namespace Zift.Sorting.Dynamic;

/// <summary>
/// Parses a directive string into one or more sort criteria.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public interface ISortDirectiveParser<T>
{
    /// <summary>
    /// Parses the given directive string into sort criteria.
    /// </summary>
    /// <param name="directives">The directive string to parse (e.g., "Name ASC, CreatedAt DESC").</param>
    /// <returns>A sequence of parsed sort criteria.</returns>
    IEnumerable<ISortCriterion<T>> Parse(string directives);
}
