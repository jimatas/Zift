namespace Zift.Filtering.Dynamic;

public class FilterCondition(PropertyPath property, ComparisonOperator @operator, LiteralValue value) : FilterTerm
{
    public PropertyPath Property { get; } = property.ThrowIfNull();
    public ComparisonOperator Operator { get; } = @operator;
    public LiteralValue Value { get; } = value;

    public override Expression<Func<T, bool>> ToExpression<T>()
    {
        return new FilterExpressionBuilder<T>(this).BuildExpression();
    }
}
