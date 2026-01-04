namespace Zift.Pagination.Cursor;

internal sealed class CursorValues(IReadOnlyList<object?> values)
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public IReadOnlyList<object?> Values { get; } = values;

    public string Encode()
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(Values, _serializerOptions);

        return Convert.ToBase64String(bytes);
    }

    public static CursorValues Decode(string cursor, IReadOnlyList<Type> cursorValueTypes)
    {
        var bytes = Convert.FromBase64String(cursor);

        var elements = JsonSerializer.Deserialize<JsonElement[]>(bytes, _serializerOptions)
            ?? throw new FormatException("Cursor is not a valid JSON array.");

        if (elements.Length != cursorValueTypes.Count)
        {
            throw new FormatException(
                $"Cursor contains {elements.Length} element(s), expected {cursorValueTypes.Count}.");
        }

        var values = new object?[elements.Length];

        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            var type = cursorValueTypes[i];

            values[i] = element.Deserialize(type, _serializerOptions);
        }

        return new CursorValues(values);
    }
}
