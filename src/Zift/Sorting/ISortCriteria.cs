namespace Zift.Sorting;

/// <summary>
/// Represents sorting criteria that can be applied to a query.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public interface ISortCriteria<T> : ICriteria<T>, IEnumerable<ISortCriterion<T>>;
