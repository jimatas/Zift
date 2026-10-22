namespace Zift.Querying.ExpressionBuilding;

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
        var value = ParseDecimal(text, targetType);

        if (value != decimal.Truncate(value))
        {
            throw new InvalidOperationException(
                $"The numeric literal '{text}' is not an integer and cannot be converted to '{targetType.Name}'.");
        }

        try
        {
            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch (OverflowException)
        {
            throw OutOfRange(text, targetType);
        }
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
