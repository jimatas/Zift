namespace Zift.EntityFrameworkCore;

public static class QueryCriteriaBuilderExtensions
{
    public static QueryCriteriaBuilder<T> FilterBy<T>(this QueryCriteriaBuilder<T> builder,
        Expression<Func<T, bool>> predicate)
    {
        builder.QueryCriteria.Filter = new Filtering.PredicateFilterCriteria<T>(predicate);

        return builder;
    }

    public static QueryCriteriaBuilder<T> SortBy<T>(this QueryCriteriaBuilder<T> builder,
        string property, Sorting.SortDirection direction = Sorting.SortDirection.Ascending)
    {
        builder.QueryCriteria.SortCriteria.Add(new Sorting.SortCriterion<T>(property, direction));

        return builder;
    }

    public static QueryCriteriaBuilder<T> SortBy<T, TProperty>(this QueryCriteriaBuilder<T> builder,
        Expression<Func<T, TProperty>> property,
        Sorting.SortDirection direction = Sorting.SortDirection.Ascending)
    {
        builder.QueryCriteria.SortCriteria.Add(new Sorting.SortCriterion<T, TProperty>(property, direction));

        return builder;
    }

    public static QueryCriteriaBuilder<T> SortBy<T>(this QueryCriteriaBuilder<T> builder,
        string directives,
        Sorting.Dynamic.ISortDirectiveParser<T> parser)
    {
        foreach (var criterion in parser.Parse(directives))
        {
            builder.QueryCriteria.SortCriteria.Add(criterion);
        }

        return builder;
    }
}
