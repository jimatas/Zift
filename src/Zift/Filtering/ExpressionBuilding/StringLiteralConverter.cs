namespace Zift.Filtering.ExpressionBuilding;

internal static class StringLiteralConverter
{
    private static readonly Dictionary<Type, Func<string, object>> _parsers = new()
    {
        [typeof(DateTime)] = s =>
            DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),

        [typeof(DateTimeOffset)] = s =>
            DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),

        [typeof(Guid)] = s => Guid.Parse(s),

        [typeof(DateOnly)] = s =>
            DateOnly.Parse(s, CultureInfo.InvariantCulture),

        [typeof(TimeOnly)] = s =>
            TimeOnly.Parse(s, CultureInfo.InvariantCulture)
    };

    public static object Convert(string value, Type targetType)
    {
        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        if (_parsers.TryGetValue(targetType, out var parser))
        {
            return Parse(parser, value, targetType);
        }

        if (targetType.IsEnum)
        {
            return ParseEnum(value, targetType);
        }

        throw new InvalidFilterException(
            $"String values cannot be converted to '{targetType.Name}'.");
    }

    private static object Parse(Func<string, object> parser, string value, Type targetType)
    {
        try
        {
            return parser(value);
        }
        catch (FormatException exception)
        {
            throw new InvalidFilterException(
                $"'{value}' is not a valid {targetType.Name}.",
                exception);
        }
    }

    private static object ParseEnum(string value, Type enumType)
    {
        var names = value.Split(',', StringSplitOptions.TrimEntries);
        var definedNames = Enum.GetNames(enumType);

        var isFlags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
        var isValid = (isFlags || names.Length == 1) && names.All(definedNames.Contains);

        if (!isValid)
        {
            throw new InvalidFilterException(
                $"'{value}' is not a defined value of '{enumType.Name}'.");
        }

        return Enum.Parse(enumType, value, ignoreCase: false);
    }
}
