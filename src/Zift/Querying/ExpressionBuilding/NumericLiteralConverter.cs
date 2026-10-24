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

        throw new InvalidFilterException(
            $"Numeric values cannot be converted to '{targetType.Name}'.");
    }

    private static object ConvertToIntegral(string text, Type targetType)
    {
        if (!NumberScanner.TryScan(text, out var parts) || parts.Length != text.Length)
        {
            throw new InvalidFilterException($"The numeric literal '{text}' is not valid.");
        }

        if (!IsIntegral(parts))
        {
            throw new InvalidFilterException(
                $"The numeric literal '{text}' is not an integer and cannot be converted to '{targetType.Name}'.");
        }

        var value = ParseDecimal(text, targetType);

        try
        {
            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch (OverflowException exception)
        {
            throw OutOfRange(text, targetType, exception);
        }
    }

    private static bool IsIntegral(NumberParts parts)
    {
        var digits = parts.IntegerDigits + parts.FractionDigits;
        var significantDigits = digits.TrimEnd('0');

        if (significantDigits.Length == 0)
        {
            return true;
        }

        var minimumExponent = parts.FractionDigits.Length
            - (digits.Length - significantDigits.Length);

        var exponent = 0;

        foreach (var digit in parts.ExponentDigits)
        {
            var digitValue = digit - '0';

            if (exponent > (int.MaxValue - digitValue) / 10)
            {
                return !parts.IsExponentNegative;
            }

            exponent = exponent * 10 + digitValue;
        }

        if (parts.IsExponentNegative)
        {
            exponent = -exponent;
        }

        return exponent >= minimumExponent;
    }

    private static decimal ParseDecimal(string text, Type targetType)
    {
        try
        {
            return decimal.Parse(text, Style, CultureInfo.InvariantCulture);
        }
        catch (OverflowException exception)
        {
            throw OutOfRange(text, targetType, exception);
        }
    }

    private static InvalidFilterException OutOfRange(string text, Type targetType, Exception? innerException = null)
    {
        return new($"The numeric literal '{text}' is out of range for '{targetType.Name}'.", innerException);
    }
}
