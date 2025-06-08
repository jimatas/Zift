namespace Zift.Sorting;

/// <summary>
/// A sort criterion that applies to a query using a lambda expression selector.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public class SortCriterion<T> : SortCriterion, ISortCriterion<T>
{
    private const string NoPropertyName = "[None]";
    private static readonly ConcurrentDictionary<string, MethodInfo> _sortMethodCache = new();

    /// <summary>
    /// Initializes a new instance of <see cref="SortCriterion{T}"/> using a property name and sort direction.
    /// </summary>
    /// <param name="property">The name of the property to sort by.</param>
    /// <param name="direction">The sort direction.</param>
    public SortCriterion(string property, SortDirection direction)
        : base(property, direction)
    {
        Property = property.ToPropertySelector<T>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SortCriterion{T}"/> using a lambda expression and sort direction.
    /// </summary>
    /// <param name="property">The lambda expression for the property to sort by.</param>
    /// <param name="direction">The sort direction.</param>
    protected SortCriterion(LambdaExpression property, SortDirection direction)
        : base(property.ThrowIfNull().ToPropertyPath() ?? NoPropertyName, direction)
    {
        Property = property;
    }

    /// <inheritdoc/>
    public new LambdaExpression Property { get; }

    /// <inheritdoc/>
    public virtual IOrderedQueryable<T> ApplyTo(IQueryable<T> query)
    {
        query.ThrowIfNull();

        var sortMethod = GetSortMethod(initial: true);

        return (IOrderedQueryable<T>)sortMethod.Invoke(
            obj: null,
            parameters: [query, Property])!;
    }

    /// <inheritdoc/>
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
        var ascending = Direction == SortDirection.Ascending;

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
