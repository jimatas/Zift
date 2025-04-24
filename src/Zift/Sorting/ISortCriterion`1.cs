namespace Zift.Sorting;

public interface ISortCriterion<T> : ISortCriterion
{
    new LambdaExpression Property { get; }

    IOrderedQueryable<T> ApplyTo(IQueryable<T> query);
    IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> sortedQuery);
}
