namespace Zift.Filtering.Dynamic;

public readonly record struct ComparisonOperatorDefinition(string Symbol, string Name)
{
    private static readonly Dictionary<string, MethodInfo> _stringComparisonMethods = new()
    {
        [nameof(string.Contains)] = ResolveStringComparisonMethod(nameof(string.Contains)),
        [nameof(string.StartsWith)] = ResolveStringComparisonMethod(nameof(string.StartsWith)),
        [nameof(string.EndsWith)] = ResolveStringComparisonMethod(nameof(string.EndsWith))
    };

    public IReadOnlySet<string> SupportedModifiers { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public bool IsListOperator { get; init; } = false;
    public bool IsStringOnly { get; init; } = false;

    public override string ToString() => Symbol;

    public Expression ToComparisonExpression(Expression leftOperand, Expression rightOperand)
    {
        if (this == ComparisonOperatorDefinitions.Equal) return Expression.Equal(leftOperand, rightOperand);
        if (this == ComparisonOperatorDefinitions.NotEqual) return Expression.NotEqual(leftOperand, rightOperand);
        if (this == ComparisonOperatorDefinitions.GreaterThan) return Expression.GreaterThan(leftOperand, rightOperand);
        if (this == ComparisonOperatorDefinitions.GreaterThanOrEqual) return Expression.GreaterThanOrEqual(leftOperand, rightOperand);
        if (this == ComparisonOperatorDefinitions.LessThan) return Expression.LessThan(leftOperand, rightOperand);
        if (this == ComparisonOperatorDefinitions.LessThanOrEqual) return Expression.LessThanOrEqual(leftOperand, rightOperand);
        if (this == ComparisonOperatorDefinitions.Contains && leftOperand.Type == typeof(string)) return Expression.Call(leftOperand, _stringComparisonMethods[nameof(string.Contains)], rightOperand);
        if (this == ComparisonOperatorDefinitions.StartsWith && leftOperand.Type == typeof(string)) return Expression.Call(leftOperand, _stringComparisonMethods[nameof(string.StartsWith)], rightOperand);
        if (this == ComparisonOperatorDefinitions.EndsWith && leftOperand.Type == typeof(string)) return Expression.Call(leftOperand, _stringComparisonMethods[nameof(string.EndsWith)], rightOperand);

        throw new NotSupportedException($"The operator '{this}' is not supported.");
    }

    public bool IsImplementedAsMethodCall()
    {
        return this == ComparisonOperatorDefinitions.Contains
            || this == ComparisonOperatorDefinitions.StartsWith
            || this == ComparisonOperatorDefinitions.EndsWith;
    }

    private static MethodInfo ResolveStringComparisonMethod(string methodName)
    {
        return typeof(string)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method =>
                method.Name == methodName
                && method.GetParameters().Length == 1
                && method.GetParameters().Single().ParameterType == typeof(string));
    }
}
