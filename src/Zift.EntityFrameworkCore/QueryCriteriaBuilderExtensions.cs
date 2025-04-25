namespace Zift.EntityFrameworkCore;

public static class QueryCriteriaBuilderExtensions
{
    public static QueryCriteriaBuilder<T> FilterBy<T>(this QueryCriteriaBuilder<T> builder,
        Expression<Func<T, bool>> predicate)
    {
        return builder.FilterBy(new Filtering.PredicateFilterCriteria<T>(predicate));
    }

    public static QueryCriteriaBuilder<T> SortBy<T>(this QueryCriteriaBuilder<T> builder,
        string property, Sorting.SortDirection direction = Sorting.SortDirection.Ascending)
    {
        return builder.SortBy(new Sorting.SortCriterion<T>(property, direction));
    }

    public static QueryCriteriaBuilder<T> SortBy<T, TProperty>(this QueryCriteriaBuilder<T> builder,
        Expression<Func<T, TProperty>> property,
        Sorting.SortDirection direction = Sorting.SortDirection.Ascending)
    {
        return builder.SortBy(new Sorting.SortCriterion<T, TProperty>(property, direction));
    }

    public static QueryCriteriaBuilder<T> SortBy<T>(this QueryCriteriaBuilder<T> builder,
        string sortString,
        Sorting.Dynamic.ISortDirectiveParser<T> parser)
    {
        foreach (var criterion in parser.Parse(sortString))
        {
            builder.SortBy(criterion);
        }

        return builder;
    }
}
