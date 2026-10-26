namespace Zift.Filtering.Literals;

public sealed class NumberScannerTests
{
    [Theory]
    [InlineData("0", false, "0", "", "", false)]
    [InlineData("42", false, "42", "", "", false)]
    [InlineData("+7", false, "7", "", "", false)]
    [InlineData("-7", true, "7", "", "", false)]
    [InlineData("1.50", false, "1", "50", "", false)]
    [InlineData(".5", false, "", "5", "", false)]
    [InlineData("-.5", true, "", "5", "", false)]
    [InlineData("1e3", false, "1", "", "3", false)]
    [InlineData("1E3", false, "1", "", "3", false)]
    [InlineData("1e+3", false, "1", "", "3", false)]
    [InlineData("1e-3", false, "1", "", "3", true)]
    [InlineData("-1.5e-10", true, "1", "5", "10", true)]
    [InlineData(".5e2", false, "", "5", "2", false)]
    public void TryScan_ValidNumber_ReturnsParts(
        string text,
        bool isNegative,
        string integerDigits,
        string fractionDigits,
        string exponentDigits,
        bool isExponentNegative)
    {
        var scanned = NumberScanner.TryScan(text, out var parts);

        Assert.True(scanned);
        Assert.Equal(text.Length, parts.Length);
        Assert.Equal(isNegative, parts.IsNegative);
        Assert.Equal(integerDigits, parts.IntegerDigits);
        Assert.Equal(fractionDigits, parts.FractionDigits);
        Assert.Equal(exponentDigits, parts.ExponentDigits);
        Assert.Equal(isExponentNegative, parts.IsExponentNegative);
    }

    [Theory]
    [InlineData("12abc", 2)]
    [InlineData("12 ", 2)]
    [InlineData("1.5.5", 3)]
    [InlineData("1e5.5", 3)]
    [InlineData("007", 3)]
    [InlineData("3000000000)", 10)]
    public void TryScan_StopsAtFirstCharacterThatCannotContinueTheNumber(string text, int expectedLength)
    {
        var scanned = NumberScanner.TryScan(text, out var parts);

        Assert.True(scanned);
        Assert.Equal(expectedLength, parts.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData("1.")]
    [InlineData("1.e5")]
    [InlineData("1e")]
    [InlineData("1e+")]
    [InlineData("1e-")]
    [InlineData("e5")]
    [InlineData("+e5")]
    [InlineData("abc")]
    [InlineData(" 1")]
    public void TryScan_InvalidNumber_ReturnsFalse(string text)
    {
        Assert.False(NumberScanner.TryScan(text, out _));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("12abc", true)]
    [InlineData(".5", true)]
    [InlineData("+5", true)]
    [InlineData("-5", true)]
    [InlineData("+.5", true)]
    [InlineData("-.5", true)]
    [InlineData("1.", true)]
    [InlineData("", false)]
    [InlineData("+", false)]
    [InlineData("-", false)]
    [InlineData(".", false)]
    [InlineData("+.", false)]
    [InlineData("-x", false)]
    [InlineData(".x", false)]
    [InlineData("..5", false)]
    [InlineData("+-5", false)]
    [InlineData("e5", false)]
    [InlineData(" 1", false)]
    public void StartsNumber_ReportsWhetherANumberBeginsAtTheStart(string text, bool expected)
    {
        Assert.Equal(expected, NumberScanner.StartsNumber(text));
    }

    [Theory]
    [InlineData("1")]
    [InlineData(".5")]
    [InlineData("-5")]
    [InlineData("+.5e3")]
    [InlineData("12abc")]
    public void StartsNumber_IsTrueForEverythingTryScanAccepts(string text)
    {
        Assert.True(NumberScanner.TryScan(text, out _));
        Assert.True(NumberScanner.StartsNumber(text));
    }

    [Fact]
    public void TryScan_ReadsDigitsWithoutLimit()
    {
        var digits = new string('7', 5000);

        var scanned = NumberScanner.TryScan(digits + "e" + digits, out var parts);

        Assert.True(scanned);
        Assert.Equal(digits, parts.IntegerDigits);
        Assert.Equal(digits, parts.ExponentDigits);
    }
}
