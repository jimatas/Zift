namespace Zift.Querying.Literals;

internal static class NumberScanner
{
    public static bool StartsNumber(ReadOnlySpan<char> text)
    {
        var i = 0;

        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            i++;
        }

        if (i < text.Length && text[i] == '.')
        {
            i++;
        }

        return i < text.Length && char.IsAsciiDigit(text[i]);
    }

    public static bool TryScan(ReadOnlySpan<char> text, out NumberParts parts)
    {
        parts = default;

        var i = 0;
        var isNegative = false;

        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            isNegative = text[i] == '-';
            i++;
        }

        var integerDigits = ReadDigits(text, ref i);
        var fractionDigits = string.Empty;

        if (i < text.Length && text[i] == '.')
        {
            i++;
            fractionDigits = ReadDigits(text, ref i);

            if (fractionDigits.Length == 0)
            {
                return false;
            }
        }

        if (integerDigits.Length == 0 && fractionDigits.Length == 0)
        {
            return false;
        }

        var exponentDigits = string.Empty;
        var isExponentNegative = false;

        if (i < text.Length && (text[i] == 'e' || text[i] == 'E'))
        {
            i++;

            if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            {
                isExponentNegative = text[i] == '-';
                i++;
            }

            exponentDigits = ReadDigits(text, ref i);

            if (exponentDigits.Length == 0)
            {
                return false;
            }
        }

        parts = new NumberParts(
            i,
            isNegative,
            integerDigits,
            fractionDigits,
            exponentDigits,
            isExponentNegative);

        return true;
    }

    private static string ReadDigits(ReadOnlySpan<char> text, ref int index)
    {
        var start = index;

        while (index < text.Length && char.IsAsciiDigit(text[index]))
        {
            index++;
        }

        return text[start..index].ToString();
    }
}
