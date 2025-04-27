namespace Zift.Sorting;

public class SortCriteriaBuilder<T>(SortCriteria<T> sortCriteria)
{
    private readonly SortCriteria<T> _sortCriteria = sortCriteria.ThrowIfNull();

    public SortCriteriaBuilder<T> Ascending(string property)
    {
        _sortCriteria.Add(new SortCriterion<T>(property, SortDirection.Ascending));

        return this;
    }

    public SortCriteriaBuilder<T> Ascending<TProperty>(Expression<Func<T, TProperty>> property)
    {
        _sortCriteria.Add(new SortCriterion<T, TProperty>(property, SortDirection.Ascending));

        return this;
    }

    public SortCriteriaBuilder<T> Descending(string property)
    {
        _sortCriteria.Add(new SortCriterion<T>(property, SortDirection.Descending));

        return this;
    }

    public SortCriteriaBuilder<T> Descending<TProperty>(Expression<Func<T, TProperty>> property)
    {
        _sortCriteria.Add(new SortCriterion<T, TProperty>(property, SortDirection.Descending));

        return this;
    }

    public SortCriteriaBuilder<T> Clause(string directives)
    {
        return Clause(directives, new Dynamic.SortDirectiveParser<T>());
    }

    public SortCriteriaBuilder<T> Clause(string directives, Dynamic.ISortDirectiveParser<T> parser)
    {
        parser.ThrowIfNull();

        foreach (var criterion in parser.Parse(directives))
        {
            _sortCriteria.Add(criterion);
        }

        return this;
    }
}
