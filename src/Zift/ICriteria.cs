namespace Zift;

/// <summary>
/// Represents criteria that can be applied to a query.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
public interface ICriteria<T>
{
    /// <summary>
    /// Applies the criteria to the given query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <returns>A query with the criteria applied.</returns>
    IQueryable<T> ApplyTo(IQueryable<T> query);
}
