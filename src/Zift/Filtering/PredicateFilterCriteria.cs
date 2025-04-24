namespace Zift.Filtering;

public class PredicateFilterCriteria<T>(Expression<Func<T, bool>> predicate) : IFilterCriteria<T>
{
    private readonly Expression<Func<T, bool>> _predicate = predicate.ThrowIfNull();

    public IQueryable<T> ApplyTo(IQueryable<T> query) => query.Where(_predicate);
}
