namespace Zift.Filtering;

/// <summary>
/// A filter criteria implementation based on a dynamic filter expression or parsed filter term.
/// </summary>
/// <typeparam name="T">The type of elements in the query.</typeparam>
/// <param name="term">The parsed filter term to use.</param>
/// <param name="options">Optional filter options for expression generation.</param>
public class DynamicFilterCriteria<T>(Dynamic.FilterTerm term, Dynamic.FilterOptions? options = null)
    : PredicateFilterCriteria<T>(term.ThrowIfNull().ToExpression<T>(options))
{
    /// <summary>
    /// Initializes a new instance of <see cref="DynamicFilterCriteria{T}"/> using a dynamic filter expression string.
    /// </summary>
    /// <param name="expression">The filter expression string to parse
    /// (e.g., <c>"Rating &gt; 4 &amp;&amp; Category.Name == 'Books'"</c>).</param>
    /// <param name="options">Optional filter options for expression generation.</param>
    public DynamicFilterCriteria(string expression, Dynamic.FilterOptions? options = null)
        : this(ParseFilterTerm(expression.ThrowIfNullOrEmpty()), options)
    {
    }

    private static Dynamic.FilterGroup ParseFilterTerm(string expression)
    {
        return new Dynamic.Parsing.ExpressionParser(new(expression)).Parse();
    }
}
