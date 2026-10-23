namespace Zift.Querying.Literals;

internal readonly record struct NumberParts(
    int Length,
    bool IsNegative,
    string IntegerDigits,
    string FractionDigits,
    string ExponentDigits,
    bool ExponentIsNegative);
