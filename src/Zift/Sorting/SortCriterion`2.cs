namespace Zift.Sorting;

/// <summary>
/// A strongly typed sort criterion based on a property selector.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
/// <typeparam name="TProperty">The type of the property to sort by.</typeparam>
/// <param name="property">The lambda expression selecting the property.</param>
/// <param name="direction">The sort direction.</param>
public sealed class SortCriterion<T, TProperty>(Expression<Func<T, TProperty>> property, SortDirection direction)
    : SortCriterion<T>(property, direction)
{
    /// <summary>
    /// The strongly typed property selector.
    /// </summary>
    public new Expression<Func<T, TProperty>> Property { get; } = property;

    /// <inheritdoc/>
    public override IOrderedQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        return Direction == SortDirection.Ascending
            ? query.OrderBy(Property)
            : query.OrderByDescending(Property);
    }

    /// <inheritdoc/>
    public override IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> sortedQuery)
    {
        sortedQuery.ThrowIfNull();

        return Direction == SortDirection.Ascending
            ? sortedQuery.ThenBy(Property)
            : sortedQuery.ThenByDescending(Property);
    }
}
