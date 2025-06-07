namespace Zift.Filtering;

/// <summary>
/// A filter criteria based on a boolean predicate expression.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
/// <param name="predicate">The predicate used to filter the query.</param>
public class PredicateFilterCriteria<T>(Expression<Func<T, bool>> predicate) : IFilterCriteria<T>
{
    private readonly Expression<Func<T, bool>> _predicate = predicate.ThrowIfNull();

    /// <inheritdoc/>
    public IQueryable<T> ApplyTo(IQueryable<T> query) => query.Where(_predicate);
}
