namespace Zift.Pagination.Cursor;

internal static class CursorToken
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Encode(string fingerprint, IReadOnlyList<object?> values)
    {
        var payload = new WritePayload(fingerprint, values.ToArray());
        var json = JsonSerializer.SerializeToUtf8Bytes(payload, _options);

        return ToBase64Url(json);
    }

    public static object?[] Decode(string cursor, string expectedFingerprint, IReadOnlyList<Type> keyTypes)
    {
        var (fingerprint, cursorValues) = Parse(cursor);

        if (fingerprint != expectedFingerprint)
        {
            throw new InvalidCursorException("The cursor was created for a different ordering.");
        }

        if (cursorValues.Length != keyTypes.Count)
        {
            throw new InvalidCursorException(
                $"The cursor contains {cursorValues.Length} value(s), but the ordering has {keyTypes.Count} key(s).");
        }

        var values = new object?[keyTypes.Count];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = DeserializeValue(cursorValues[i], keyTypes[i]);
        }

        return values;
    }

    private static (string Fingerprint, JsonElement[] Values) Parse(string cursor)
    {
        ReadPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ReadPayload>(FromBase64Url(cursor), _options);
        }
        catch (Exception exception) when (IsDecodingFailure(exception))
        {
            throw new InvalidCursorException("The cursor is malformed.", exception);
        }

        return payload is { Fingerprint: { } fingerprint, Values: { } values }
            ? (fingerprint, values)
            : throw new InvalidCursorException("The cursor is incomplete.");
    }

    private static object? DeserializeValue(JsonElement element, Type keyType)
    {
        try
        {
            return element.Deserialize(keyType, _options);
        }
        catch (Exception exception) when (IsDecodingFailure(exception))
        {
            throw new InvalidCursorException("The cursor is malformed.", exception);
        }
    }

    private static bool IsDecodingFailure(Exception exception)
    {
        return exception is not InvalidCursorException and (FormatException or JsonException);
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] FromBase64Url(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var paddingLength = (4 - base64.Length % 4) % 4;
        var paddedBase64 = base64.PadRight(base64.Length + paddingLength, '=');

        return Convert.FromBase64String(paddedBase64);
    }

    private sealed record WritePayload(
        [property: JsonPropertyName("f")] string Fingerprint,
        [property: JsonPropertyName("v")] object?[] Values);

    private sealed record ReadPayload(
        [property: JsonPropertyName("f")] string? Fingerprint,
        [property: JsonPropertyName("v")] JsonElement[]? Values);
}
