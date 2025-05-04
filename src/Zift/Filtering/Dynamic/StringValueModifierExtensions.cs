namespace Zift.Filtering.Dynamic;

public static class StringValueModifierExtensions
{
    private static readonly Dictionary<StringValueModifier, string> _displayNames = new()
    {
        [StringValueModifier.IgnoreCase] = "i"
    };

    public static string ToDisplayString(this StringValueModifier modifier)
    {
        return _displayNames.TryGetValue(modifier, out var displayName) ? displayName : modifier.ToString();
    }

    public static bool TryParse(string value, out StringValueModifier result)
    {
        foreach (var (candidate, displayName) in _displayNames)
        {
            if (displayName.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = candidate;
                return true;
            }
        }

        result = default;
        return false;
    }
}
