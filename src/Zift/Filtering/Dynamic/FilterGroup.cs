namespace Zift.Filtering.Dynamic;

/// <summary>
/// A filter term that combines multiple sub-terms using a logical operator (e.g., <c>AND</c>, <c>OR</c>).
/// </summary>
public class FilterGroup(LogicalOperator @operator) : FilterTerm
{
    /// <summary>
    /// The list of filter terms in the group.
    /// </summary>
    public IList<FilterTerm> Terms { get; } = [];

    /// <summary>
    /// The logical operator that connects the terms in the group.
    /// </summary>
    public LogicalOperator Operator { get; } = @operator;

    /// <inheritdoc/>
    public override Expression<Func<T, bool>> ToExpression<T>(FilterOptions? options = null)
    {
        return new GroupedFilterExpressionBuilder<T>(this, options).BuildExpression();
    }
}
