namespace Zift.Filtering.Dynamic;

public class FilterCondition : FilterTerm
{
    public FilterCondition(PropertyPath property, ComparisonOperator @operator, object? value)
    {
        Property = property.ThrowIfNull();
        Operator = @operator;

        ValidateValueMultiplicity(@operator, value);
        Value = value;
    }

    public PropertyPath Property { get; }
    public ComparisonOperator Operator { get; }
    public object? Value { get; }

    public override Expression<Func<T, bool>> ToExpression<T>()
    {
        return new FilterExpressionBuilder<T>(this).BuildExpression();
    }

    private static void ValidateValueMultiplicity(ComparisonOperator @operator, object? value)
    {
        var isMultiValued = value is IEnumerable && value is not string;
        var isListOperator = @operator.Type == ComparisonOperatorType.In;

        if (isListOperator && !isMultiValued)
        {
            throw new ArgumentException(
                $"Operator '{@operator.Type.Symbol}' requires a list of values.",
                nameof(value));
        }

        if (!isListOperator && isMultiValued)
        {
            throw new ArgumentException(
                $"Operator '{@operator.Type.Symbol}' does not support a list of values.",
                nameof(value));
        }
    }
}
