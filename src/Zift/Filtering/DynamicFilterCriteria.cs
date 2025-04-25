namespace Zift.Filtering;

using Dynamic;
using Dynamic.Parsing;

public class DynamicFilterCriteria<T>(FilterTerm term)
    : PredicateFilterCriteria<T>(term.ThrowIfNull().ToExpression<T>())
{
    public DynamicFilterCriteria(string expression)
        : this(ParseFilterTerm(expression.ThrowIfNullOrEmpty()))
    {
    }

    private static FilterGroup ParseFilterTerm(string expression)
    {
        return new ExpressionParser(new(expression)).Parse();
    }
}
