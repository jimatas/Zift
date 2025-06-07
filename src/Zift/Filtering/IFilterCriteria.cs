namespace Zift.Filtering;

/// <summary>
/// Represents filtering criteria that can be applied to a query.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public interface IFilterCriteria<T> : ICriteria<T>;
