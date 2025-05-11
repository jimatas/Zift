namespace Zift.Filtering.Dynamic;

public class FilterGroup(LogicalOperator @operator) : FilterTerm
{
    public IList<FilterTerm> Terms { get; } = new List<FilterTerm>();
    public LogicalOperator Operator { get; } = @operator;

    public override Expression<Func<T, bool>> ToExpression<T>(FilterOptions? options = null)
    {
        return new GroupedFilterExpressionBuilder<T>(this, options).BuildExpression();
    }
}
