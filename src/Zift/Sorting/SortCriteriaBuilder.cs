namespace Zift.Sorting;

/// <summary>
/// Provides a fluent API for configuring a <see cref="SortCriteria{T}"/> instance.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
/// <param name="criteria">The criteria instance to configure.</param>
public class SortCriteriaBuilder<T>(SortCriteria<T> criteria)
{
    private readonly SortCriteria<T> _criteria = criteria.ThrowIfNull();

    /// <summary>
    /// Adds an ascending sort based on a property name.
    /// </summary>
    /// <param name="property">The name of the property to sort by.</param>
    /// <returns>The current builder instance.</returns>
    public SortCriteriaBuilder<T> Ascending(string property)
    {
        _criteria.Add(new SortCriterion<T>(property, SortDirection.Ascending));

        return this;
    }

    /// <summary>
    /// Adds an ascending sort based on a property selector.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="property">The property selector.</param>
    /// <returns>The current builder instance.</returns>
    public SortCriteriaBuilder<T> Ascending<TProperty>(Expression<Func<T, TProperty>> property)
    {
        _criteria.Add(new SortCriterion<T, TProperty>(property, SortDirection.Ascending));

        return this;
    }

    /// <summary>
    /// Adds a descending sort based on a property name.
    /// </summary>
    /// <param name="property">The name of the property to sort by.</param>
    /// <returns>The current builder instance.</returns>
    public SortCriteriaBuilder<T> Descending(string property)
    {
        _criteria.Add(new SortCriterion<T>(property, SortDirection.Descending));

        return this;
    }

    /// <summary>
    /// Adds a descending sort based on a property selector.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="property">The property selector.</param>
    /// <returns>The current builder instance.</returns>
    public SortCriteriaBuilder<T> Descending<TProperty>(Expression<Func<T, TProperty>> property)
    {
        _criteria.Add(new SortCriterion<T, TProperty>(property, SortDirection.Descending));

        return this;
    }

    /// <summary>
    /// Adds one or more sort criteria from a sort directive string.
    /// </summary>
    /// <param name="directives">The directive string to parse (e.g., "Name ASC, CreatedAt DESC").</param>
    /// <returns>The current builder instance.</returns>
    public SortCriteriaBuilder<T> Clause(string directives)
    {
        return Clause(directives, new Dynamic.SortDirectiveParser<T>());
    }

    /// <summary>
    /// Adds one or more sort criteria using a custom sort directive parser.
    /// </summary>
    /// <param name="directives">The directive string to parse (e.g., "Name ASC, CreatedAt DESC").</param>
    /// <param name="parser">The parser used to interpret the directives.</param>
    /// <returns>The current builder instance.</returns>
    public SortCriteriaBuilder<T> Clause(string directives, Dynamic.ISortDirectiveParser<T> parser)
    {
        parser.ThrowIfNull();

        foreach (var criterion in parser.Parse(directives))
        {
            _criteria.Add(criterion);
        }

        return this;
    }
}
