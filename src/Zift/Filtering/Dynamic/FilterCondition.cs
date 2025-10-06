namespace Zift.Filtering.Dynamic;

/// <summary>
/// A filter term representing a comparison between a property and a value using a specified operator.
/// </summary>
public sealed class FilterCondition : FilterTerm
{
    /// <summary>
    /// Initializes a new instance of <see cref="FilterCondition"/> using the specified property path,
    /// comparison operator, and value.
    /// </summary>
    /// <param name="property">The property path to compare.</param>
    /// <param name="operator">The comparison operator to apply.</param>
    /// <param name="value">The value to compare against.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is incompatible with the comparison operator (e.g., a list is required or disallowed).
    /// </exception>
    public FilterCondition(PropertyPath property, ComparisonOperator @operator, object? value)
    {
        Property = property.ThrowIfNull();
        Operator = @operator;

        ValidateValueMultiplicity(@operator, value);
        Value = value;
    }

    /// <summary>
    /// The property path the condition targets.
    /// </summary>
    public PropertyPath Property { get; }

    /// <summary>
    /// The comparison operator used in the condition.
    /// </summary>
    public ComparisonOperator Operator { get; }

    /// <summary>
    /// The value to compare the property against.
    /// </summary>
    public object? Value { get; }

    /// <inheritdoc/>
    public override Expression<Func<T, bool>> ToExpression<T>(FilterOptions? options = null) =>
        new FilterExpressionBuilder<T>(this, options).BuildExpression();

    private static void ValidateValueMultiplicity(ComparisonOperator @operator, object? value)
    {
        var isMultiValued = value is IEnumerable && value is not string;
        var isListOperator = @operator.Type == ComparisonOperatorType.In;

        if (isListOperator && !isMultiValued)
        {
            throw new ArgumentException(
                $"Operator '{@operator.Type}' requires a list of values.",
                nameof(value));
        }

        if (!isListOperator && isMultiValued)
        {
            throw new ArgumentException(
                $"Operator '{@operator.Type}' does not support a list of values.",
                nameof(value));
        }
    }
}
