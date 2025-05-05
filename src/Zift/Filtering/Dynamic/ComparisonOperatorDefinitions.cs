namespace Zift.Filtering.Dynamic;

public static class ComparisonOperatorDefinitions
{
    private static readonly Dictionary<string, ComparisonOperatorDefinition> _lookup = new(StringComparer.OrdinalIgnoreCase)
    {
        [Equal.Symbol] = Equal,
        [NotEqual.Symbol] = NotEqual,
        [GreaterThan.Symbol] = GreaterThan,
        [GreaterThanOrEqual.Symbol] = GreaterThanOrEqual,
        [LessThan.Symbol] = LessThan,
        [LessThanOrEqual.Symbol] = LessThanOrEqual,
        [Contains.Symbol] = Contains,
        [StartsWith.Symbol] = StartsWith,
        [EndsWith.Symbol] = EndsWith
    };

    public static readonly ComparisonOperatorDefinition Equal = new("==", nameof(Equal)) { SupportedModifiers = ModifierSet("i") };
    public static readonly ComparisonOperatorDefinition NotEqual = new("!=", nameof(NotEqual)) { SupportedModifiers = ModifierSet("i") };
    public static readonly ComparisonOperatorDefinition GreaterThan = new(">", nameof(GreaterThan));
    public static readonly ComparisonOperatorDefinition GreaterThanOrEqual = new(">=", nameof(GreaterThanOrEqual));
    public static readonly ComparisonOperatorDefinition LessThan = new("<", nameof(LessThan));
    public static readonly ComparisonOperatorDefinition LessThanOrEqual = new("<=", nameof(LessThanOrEqual));
    public static readonly ComparisonOperatorDefinition Contains = new("%=", nameof(Contains)) { SupportedModifiers = ModifierSet("i"), IsStringOnly = true };
    public static readonly ComparisonOperatorDefinition StartsWith = new("^=", nameof(StartsWith)) { SupportedModifiers = ModifierSet("i"), IsStringOnly = true };
    public static readonly ComparisonOperatorDefinition EndsWith = new("$=", nameof(EndsWith)) { SupportedModifiers = ModifierSet("i"), IsStringOnly = true };

    public static bool TryGet(string symbol, out ComparisonOperatorDefinition @operator) => _lookup.TryGetValue(symbol, out @operator);

    private static HashSet<string> ModifierSet(params string[] modifiers) => new(modifiers, StringComparer.OrdinalIgnoreCase);
}
