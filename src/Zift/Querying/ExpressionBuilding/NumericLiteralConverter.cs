namespace Zift.Querying.ExpressionBuilding;

using Literals;
using Types;

internal static class NumericLiteralConverter
{
    private const NumberStyles Style = NumberStyles.Float;

    public static object Convert(string text, Type targetType)
    {
        if (targetType == typeof(double))
        {
            var value = double.Parse(text, Style, CultureInfo.InvariantCulture);

            return double.IsFinite(value) ? value : throw OutOfRange(text, targetType);
        }

        if (targetType == typeof(float))
        {
            var value = float.Parse(text, Style, CultureInfo.InvariantCulture);

            return float.IsFinite(value) ? value : throw OutOfRange(text, targetType);
        }

        if (targetType == typeof(decimal))
        {
            return ParseDecimal(text, targetType);
        }

        if (targetType.IsNumeric())
        {
            return ConvertToIntegral(text, targetType);
        }

        throw new InvalidOperationException(
            $"Numeric values cannot be converted to '{targetType.Name}'.");
    }

    private static object ConvertToIntegral(string text, Type targetType)
    {
        if (!NumberScanner.TryScan(text, out var parts) || parts.Length != text.Length)
        {
            throw new FormatException($"The numeric literal '{text}' is not valid.");
        }

        if (!IsIntegral(parts))
        {
            throw new InvalidOperationException(
                $"The numeric literal '{text}' is not an integer and cannot be converted to '{targetType.Name}'.");
        }

        var value = ParseDecimal(text, targetType);

        try
        {
            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch (OverflowException)
        {
            throw OutOfRange(text, targetType);
        }
    }

    private static bool IsIntegral(NumberParts parts)
    {
        var digits = parts.IntegerDigits + parts.FractionDigits;
        var significant = digits.TrimEnd('0');

        if (significant.Length == 0)
        {
            return true;
        }

        var exponent = parts.ExponentDigits.Length == 0
            ? BigInteger.Zero
            : BigInteger.Parse(parts.ExponentDigits, CultureInfo.InvariantCulture);

        if (parts.ExponentIsNegative)
        {
            exponent = -exponent;
        }

        var trailingZeros = digits.Length - significant.Length;

        return exponent - parts.FractionDigits.Length + trailingZeros >= 0;
    }

    private static decimal ParseDecimal(string text, Type targetType)
    {
        try
        {
            return decimal.Parse(text, Style, CultureInfo.InvariantCulture);
        }
        catch (OverflowException)
        {
            throw OutOfRange(text, targetType);
        }
    }

    private static InvalidOperationException OutOfRange(string text, Type targetType)
    {
        return new($"The numeric literal '{text}' is out of range for '{targetType.Name}'.");
    }
}
