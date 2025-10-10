namespace Zift.Filtering.Dynamic;

/// <summary>
/// Represents a type of comparison operator used in filter expressions.
/// </summary>
/// <param name="Symbol">The symbolic representation of the operator (e.g., <c>"=="</c>, <c>"in"</c>).</param>
public readonly record struct ComparisonOperatorType(string Symbol)
{
    private static readonly IReadOnlySet<string> _ignoreCase = new HashSet<string>(["i"], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Represents the equality operator (<c>==</c>).
    /// </summary>
    public static readonly ComparisonOperatorType Equal = new("==") { SupportedModifiers = _ignoreCase };

    /// <summary>
    /// Represents the inequality operator (<c>!=</c>).
    /// </summary>
    public static readonly ComparisonOperatorType NotEqual = new("!=") { SupportedModifiers = _ignoreCase };

    /// <summary>
    /// Represents the greater-than operator (<c>&gt;</c>).
    /// </summary>
    public static readonly ComparisonOperatorType GreaterThan = new(">");

    /// <summary>
    /// Represents the greater-than-or-equal operator (<c>&gt;=</c>).
    /// </summary>
    public static readonly ComparisonOperatorType GreaterThanOrEqual = new(">=");

    /// <summary>
    /// Represents the less-than operator (<c>&lt;</c>).
    /// </summary>
    public static readonly ComparisonOperatorType LessThan = new("<");

    /// <summary>
    /// Represents the less-than-or-equal operator (<c>&lt;=</c>).
    /// </summary>
    public static readonly ComparisonOperatorType LessThanOrEqual = new("<=");

    /// <summary>
    /// Represents a contains operation (<c>%=</c>) on string values.
    /// </summary>
    public static readonly ComparisonOperatorType Contains = new("%=") { SupportedModifiers = _ignoreCase };

    /// <summary>
    /// Represents a starts-with operation (<c>^=</c>) on string values.
    /// </summary>
    public static readonly ComparisonOperatorType StartsWith = new("^=") { SupportedModifiers = _ignoreCase };

    /// <summary>
    /// Represents an ends-with operation (<c>$=</c>) on string values.
    /// </summary>
    public static readonly ComparisonOperatorType EndsWith = new("$=") { SupportedModifiers = _ignoreCase };

    /// <summary>
    /// Represents an inclusion check (<c>in</c>) for values in a collection.
    /// </summary>
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

    /// <summary>
    /// Attempts to parse a symbol into a <see cref="ComparisonOperatorType"/>.
    /// </summary>
    /// <param name="symbol">The symbol to parse (e.g., <c>"=="</c>, <c>"%="</c>).</param>
    /// <param name="result">The parsed operator type.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string symbol, out ComparisonOperatorType result) =>
        _bySymbol.TryGetValue(symbol, out result);

    /// <summary>
    /// The set of supported modifiers for this operator.
    /// </summary>
    public IReadOnlySet<string> SupportedModifiers { get; init; } = EmptySet<string>.Instance;

    /// <inheritdoc/>
    public override string ToString() => Symbol;

    /// <summary>
    /// Builds a comparison expression for the given operands.
    /// </summary>
    /// <param name="leftOperand">The left-hand operand.</param>
    /// <param name="rightOperand">The right-hand operand.</param>
    /// <returns>An expression representing the comparison.</returns>
    public Expression ToComparisonExpression(Expression leftOperand, Expression rightOperand) =>
        this switch
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
            var op when op == In && rightOperand.Type.IsCollectionType() =>
                Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [leftOperand.Type], rightOperand, leftOperand),
            _ => throw new NotSupportedException($"The operator '{this}' is not supported or cannot be applied to the given operands.")
        };

    private static MethodInfo ResolveStringComparisonMethod(string name) =>
        typeof(string)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method =>
                method.Name == name &&
                method.GetParameters().Length == 1 &&
                method.GetParameters().Single().ParameterType == typeof(string));
}
