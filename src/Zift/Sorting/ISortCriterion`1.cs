namespace Zift.Sorting;

/// <summary>
/// Represents a typed sort criterion that can be applied to a query.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public interface ISortCriterion<T> : ISortCriterion
{
    /// <summary>
    /// The lambda expression representing the property to sort by.
    /// </summary>
    new LambdaExpression Property { get; }

    /// <summary>
    /// Applies this sort criterion to an unsorted query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <returns>The ordered query.</returns>
    IOrderedQueryable<T> ApplyTo(IQueryable<T> query);

    /// <summary>
    /// Applies this sort criterion to an already ordered query.
    /// </summary>
    /// <param name="sortedQuery">The previously ordered query.</param>
    /// <returns>The query with additional ordering applied.</returns>
    IOrderedQueryable<T> ApplyTo(IOrderedQueryable<T> sortedQuery);
}
