namespace Zift.Querying.ExpressionBuilding;

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
            return parser(value);
        }

        if (targetType.IsEnum)
        {
            return ParseEnum(value, targetType);
        }

        throw new InvalidOperationException(
            $"String values cannot be converted to '{targetType.Name}'.");
    }

    private static object ParseEnum(string value, Type enumType)
    {
        var names = value.Split(',', StringSplitOptions.TrimEntries);
        var definedNames = Enum.GetNames(enumType);

        var isFlags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
        var isValid = (isFlags || names.Length == 1) && names.All(definedNames.Contains);

        if (!isValid)
        {
            throw new ArgumentException(
                $"'{value}' is not a defined value of '{enumType.Name}'.");
        }

        return Enum.Parse(enumType, value, ignoreCase: false);
    }
}
