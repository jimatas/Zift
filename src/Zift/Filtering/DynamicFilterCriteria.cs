namespace Zift.Filtering;

public class DynamicFilterCriteria<T>(Dynamic.FilterTerm term)
    : PredicateFilterCriteria<T>(term.ThrowIfNull().ToExpression<T>())
{
    public DynamicFilterCriteria(string expression)
        : this(ParseFilterTerm(expression.ThrowIfNullOrEmpty()))
    {
    }

    private static Dynamic.FilterGroup ParseFilterTerm(string expression)
    {
        return new Dynamic.Parsing.ExpressionParser(new(expression)).Parse();
    }
}
