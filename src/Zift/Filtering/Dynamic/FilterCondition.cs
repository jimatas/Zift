namespace Zift.Filtering.Dynamic;

public class FilterCondition(PropertyPath property, ComparisonOperator @operator, object? value) : FilterTerm
{
    public PropertyPath Property { get; } = property.ThrowIfNull();
    public ComparisonOperator Operator { get; } = @operator;
    public object? Value { get; } = value;

    public override Expression<Func<T, bool>> ToExpression<T>()
    {
        return new FilterExpressionBuilder<T>(this).BuildExpression();
    }
}
