namespace Zift.Sorting;

public class SortCriterion<T> : SortCriterion, ISortCriterion<T>
{
    private const string NoPropertyName = "[None]";

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
        var method = GetSortMethod(initial: true);

        return (IOrderedQueryable<T>)method.Invoke(
            obj: null,
            parameters: [query, Property])!;
    }

    public virtual IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> sortedQuery)
    {
        var method = GetSortMethod(initial: false);

        return (IOrderedQueryable<T>)method.Invoke(
            obj: null,
            parameters: [sortedQuery, Property])!;
    }

    private MethodInfo GetSortMethod(bool initial)
    {
        var methodName = GetSortMethodName(initial);

        return typeof(Queryable)
            .GetMethods()
            .Single(method => method.Name == methodName && method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), Property.ReturnType);
    }

    private string GetSortMethodName(bool initial)
    {
        return initial
            ? Direction is SortDirection.Ascending
                ? nameof(Queryable.OrderBy)
                : nameof(Queryable.OrderByDescending)
            : Direction is SortDirection.Ascending
                ? nameof(Queryable.ThenBy)
                : nameof(Queryable.ThenByDescending);
    }
}
