namespace Zift.Filtering;

public class DynamicFilterCriteria<T>(Dynamic.FilterTerm term, Dynamic.FilterOptions? options = null)
    : PredicateFilterCriteria<T>(term.ThrowIfNull().ToExpression<T>(options))
{
    public DynamicFilterCriteria(string expression, Dynamic.FilterOptions? options = null)
        : this(ParseFilterTerm(expression.ThrowIfNullOrEmpty()), options)
    {
    }

    private static Dynamic.FilterGroup ParseFilterTerm(string expression)
    {
        return new Dynamic.Parsing.ExpressionParser(new(expression)).Parse();
    }
}
