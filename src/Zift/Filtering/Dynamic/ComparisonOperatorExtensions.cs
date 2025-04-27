namespace Zift.Filtering.Dynamic;

public static class ComparisonOperatorExtensions
{
    private static readonly ConcurrentDictionary<string, MethodInfo> _comparisonMethodCache = new();

    public static Expression ToComparisonExpression(
        this ComparisonOperator @operator,
        Expression leftOperand,
        Expression rightOperand)
    {
        return @operator switch
        {
            ComparisonOperator.Equal => Expression.Equal(leftOperand, rightOperand),
            ComparisonOperator.NotEqual => Expression.NotEqual(leftOperand, rightOperand),
            ComparisonOperator.GreaterThan => Expression.GreaterThan(leftOperand, rightOperand),
            ComparisonOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(leftOperand, rightOperand),
            ComparisonOperator.LessThan => Expression.LessThan(leftOperand, rightOperand),
            ComparisonOperator.LessThanOrEqual => Expression.LessThanOrEqual(leftOperand, rightOperand),
            ComparisonOperator.Contains when leftOperand.Type == typeof(string) => Expression.Call(leftOperand, GetComparisonMethod(nameof(string.Contains)), rightOperand),
            ComparisonOperator.StartsWith when leftOperand.Type == typeof(string) => Expression.Call(leftOperand, GetComparisonMethod(nameof(string.StartsWith)), rightOperand),
            ComparisonOperator.EndsWith when leftOperand.Type == typeof(string) => Expression.Call(leftOperand, GetComparisonMethod(nameof(string.EndsWith)), rightOperand),
            _ => throw new NotSupportedException($"The operator '{@operator}' is not supported.")
        };
    }

    public static bool IsImplementedAsMethodCall(this ComparisonOperator @operator)
    {
        return @operator is ComparisonOperator.Contains
            or ComparisonOperator.StartsWith
            or ComparisonOperator.EndsWith;
    }

    private static MethodInfo GetComparisonMethod(string name)
    {
        return _comparisonMethodCache.GetOrAdd(name, ResolveComparisonMethod);
    }

    private static MethodInfo ResolveComparisonMethod(string name)
    {
        return typeof(string)
            .GetMethods()
            .Single(method =>
                method.Name == name
                && method.GetParameters().Length == 1
                && method.GetParameters().Single().ParameterType == typeof(string));
    }
}
