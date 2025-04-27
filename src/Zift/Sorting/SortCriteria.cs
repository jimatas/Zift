namespace Zift.Sorting;

public class SortCriteria<T> : ISortCriteria<T>
{
    private readonly List<ISortCriterion<T>> _criteria = [];

    public void Add(ISortCriterion<T> criterion)
    {
        _criteria.Add(criterion.ThrowIfNull());
    }

    public IEnumerator<ISortCriterion<T>> GetEnumerator() => _criteria.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
