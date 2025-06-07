namespace Zift.Sorting;

/// <summary>
/// Default implementation of <see cref="ISortCriteria{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public class SortCriteria<T> : ISortCriteria<T>
{
    private readonly List<ISortCriterion<T>> _criteria = [];

    /// <summary>
    /// Adds a sort criterion to the list.
    /// </summary>
    /// <param name="criterion">The sort criterion to add.</param>
    public void Add(ISortCriterion<T> criterion)
    {
        _criteria.Add(criterion.ThrowIfNull());
    }

    /// <inheritdoc/>
    public IEnumerator<ISortCriterion<T>> GetEnumerator() => _criteria.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        IOrderedQueryable<T>? sortedQuery = null;

        foreach (var criterion in _criteria)
        {
            sortedQuery = sortedQuery is null
                ? criterion.ApplyTo(query)
                : criterion.ApplyTo(sortedQuery);
        }

        return sortedQuery ?? query;
    }
}
