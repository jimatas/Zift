namespace Zift.Filtering.Dynamic;

public readonly record struct ComparisonOperatorType(string Symbol)
{
    private static readonly HashSet<string> _noModifiers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> _ignoreCase = new(StringComparer.OrdinalIgnoreCase) { "i" };

    public static readonly ComparisonOperatorType Equal = new("==") { SupportedModifiers = _ignoreCase };
    public static readonly ComparisonOperatorType NotEqual = new("!=") { SupportedModifiers = _ignoreCase };
    public static readonly ComparisonOperatorType GreaterThan = new(">");
    public static readonly ComparisonOperatorType GreaterThanOrEqual = new(">=");
    public static readonly ComparisonOperatorType LessThan = new("<");
    public static readonly ComparisonOperatorType LessThanOrEqual = new("<=");
    public static readonly ComparisonOperatorType Contains = new("%=") { SupportedModifiers = _ignoreCase };
    public static readonly ComparisonOperatorType StartsWith = new("^=") { SupportedModifiers = _ignoreCase };
    public static readonly ComparisonOperatorType EndsWith = new("$=") { SupportedModifiers = _ignoreCase };
    public static readonly ComparisonOperatorType In = new("in") { SupportedModifiers = _ignoreCase };

    private static readonly Dictionary<string, MethodInfo> _stringComparisonMethods = new()
    {
        [nameof(string.Contains)] = ResolveStringComparisonMethod(nameof(string.Contains)),
        [nameof(string.StartsWith)] = ResolveStringComparisonMethod(nameof(string.StartsWith)),
        [nameof(string.EndsWith)] = ResolveStringComparisonMethod(nameof(string.EndsWith))
    };

    private static readonly Dictionary<string, ComparisonOperatorType> _bySymbol = new(StringComparer.OrdinalIgnoreCase)
    {
        [Equal.Symbol] = Equal,
        [NotEqual.Symbol] = NotEqual,
        [GreaterThan.Symbol] = GreaterThan,
        [GreaterThanOrEqual.Symbol] = GreaterThanOrEqual,
        [LessThan.Symbol] = LessThan,
        [LessThanOrEqual.Symbol] = LessThanOrEqual,
        [Contains.Symbol] = Contains,
        [StartsWith.Symbol] = StartsWith,
        [EndsWith.Symbol] = EndsWith,
        [In.Symbol] = In
    };

    public static bool TryParse(string symbol, out ComparisonOperatorType result)
    {
        return _bySymbol.TryGetValue(symbol, out result);
    }

    public IReadOnlySet<string> SupportedModifiers { get; init; } = _noModifiers;

    public override string ToString() => Symbol;

    public Expression ToComparisonExpression(Expression leftOperand, Expression rightOperand)
    {
        return this switch
        {
            var op when op == Equal => Expression.Equal(leftOperand, rightOperand),
            var op when op == NotEqual => Expression.NotEqual(leftOperand, rightOperand),
            var op when op == GreaterThan => Expression.GreaterThan(leftOperand, rightOperand),
            var op when op == GreaterThanOrEqual => Expression.GreaterThanOrEqual(leftOperand, rightOperand),
            var op when op == LessThan => Expression.LessThan(leftOperand, rightOperand),
            var op when op == LessThanOrEqual => Expression.LessThanOrEqual(leftOperand, rightOperand),
            var op when op == Contains && leftOperand.Type == typeof(string) =>
                Expression.Call(leftOperand, _stringComparisonMethods[nameof(string.Contains)], rightOperand),
            var op when op == StartsWith && leftOperand.Type == typeof(string) =>
                Expression.Call(leftOperand, _stringComparisonMethods[nameof(string.StartsWith)], rightOperand),
            var op when op == EndsWith && leftOperand.Type == typeof(string) =>
                Expression.Call(leftOperand, _stringComparisonMethods[nameof(string.EndsWith)], rightOperand),
            var op when op == In =>
                Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [leftOperand.Type], rightOperand, leftOperand),
            _ => throw new NotSupportedException($"The operator '{this}' is not supported.")
        };
    }

    private static MethodInfo ResolveStringComparisonMethod(string name)
    {
        return typeof(string)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == name
                && method.GetParameters().Length == 1
                && method.GetParameters().Single().ParameterType == typeof(string));
    }
}
