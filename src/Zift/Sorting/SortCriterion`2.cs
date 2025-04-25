namespace Zift.Sorting;

public sealed class SortCriterion<T, TProperty>(Expression<Func<T, TProperty>> property, SortDirection direction)
    : SortCriterion<T>(property, direction)
{
    public new Expression<Func<T, TProperty>> Property { get; } = property;

    public override IOrderedQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        return Direction is SortDirection.Ascending
            ? query.OrderBy(Property)
            : query.OrderByDescending(Property);
    }

    public override IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> sortedQuery)
    {
        sortedQuery.ThrowIfNull();

        return Direction is SortDirection.Ascending
            ? sortedQuery.ThenBy(Property)
            : sortedQuery.ThenByDescending(Property);
    }
}
