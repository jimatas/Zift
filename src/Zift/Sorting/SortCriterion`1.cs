namespace Zift.Sorting;

public class SortCriterion<T> : SortCriterion, ISortCriterion<T>
{
    private const string NoPropertyName = "[None]";
    private static readonly ConcurrentDictionary<string, MethodInfo> _sortMethodCache = new();

    public SortCriterion(string property, SortDirection direction)
        : base(property, direction)
    {
        Property = property.ToPropertySelector<T>();
    }

    protected SortCriterion(LambdaExpression property, SortDirection direction)
        : base(property.ThrowIfNull().ToPropertyPath() ?? NoPropertyName, direction)
    {
        Property = property;
    }

    public new LambdaExpression Property { get; }

    public virtual IOrderedQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        var sortMethod = GetSortMethod(initial: true);

        return (IOrderedQueryable<T>)sortMethod.Invoke(
            obj: null,
            parameters: [query, Property])!;
    }

    public virtual IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> sortedQuery)
    {
        sortedQuery.ThrowIfNull();

        var sortMethod = GetSortMethod(initial: false);

        return (IOrderedQueryable<T>)sortMethod.Invoke(
            obj: null,
            parameters: [sortedQuery, Property])!;
    }

    private MethodInfo GetSortMethod(bool initial)
    {
        var sortMethod = _sortMethodCache.GetOrAdd(GetSortMethodName(initial), ResolveSortMethod);

        return sortMethod.MakeGenericMethod(typeof(T), Property.ReturnType);
    }

    private string GetSortMethodName(bool initial)
    {
        var ascending = Direction is SortDirection.Ascending;

        return initial
            ? ascending
                ? nameof(Queryable.OrderBy)
                : nameof(Queryable.OrderByDescending)
            : ascending
                ? nameof(Queryable.ThenBy)
                : nameof(Queryable.ThenByDescending);
    }

    private static MethodInfo ResolveSortMethod(string name)
    {
        return typeof(Queryable)
            .GetMethods()
            .Single(method => method.Name == name && method.GetParameters().Length == 2);
    }
}
