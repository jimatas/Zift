namespace Zift.Pagination.Cursor.Ordering;

public sealed class ExpressionDescriberTests
{
    private sealed class Scale(decimal factor)
    {
        public decimal Factor { get; } = factor;
    }

    private static string DescribeUnder(string culture, Expression expression)
    {
        var original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);

            return expression.Describe();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("sv-SE")]
    public void Describe_InlineConstants_AreCultureIndependent(string culture)
    {
        Expression<Func<int, decimal>> selector = id => id * 1.5m - 2m;

        var expected = DescribeUnder("en-US", selector.Body);

        Assert.Equal(expected, DescribeUnder(culture, selector.Body));
        Assert.Contains("1.5", expected);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("sv-SE")]
    public void Describe_CapturedValues_AreCultureIndependent(string culture)
    {
        var rate = 0.25;
        var offset = -3;
        Expression<Func<int, double>> selector = id => id * rate + offset;

        var expected = DescribeUnder("en-US", selector.Body);

        Assert.Equal(expected, DescribeUnder(culture, selector.Body));
        Assert.Contains("0.25", expected);
        Assert.Contains("-3", expected);
    }

    [Fact]
    public void Describe_CapturedValueOnObject_IsInlined()
    {
        var scale = new Scale(2.5m);
        Expression<Func<int, decimal>> selector = id => id * scale.Factor;

        Assert.Contains("2.5", selector.Body.Describe());
    }

    [Fact]
    public void Describe_DifferentCapturedValues_ProduceDifferentDescriptions()
    {
        string Describe(decimal factor)
        {
            Expression<Func<int, decimal>> selector = id => id * factor;

            return selector.Body.Describe();
        }

        Assert.NotEqual(Describe(1m), Describe(2m));
        Assert.Equal(Describe(1m), Describe(1m));
    }

    [Fact]
    public void Describe_DateTimesThatDifferBySubSeconds_ProduceDifferentDescriptions()
    {
        var first = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var second = first.AddTicks(1);

        string Describe(DateTime stamp)
        {
            Expression<Func<int, bool>> selector = id => stamp.Ticks > id;

            return selector.Body.Describe();
        }

        Assert.NotEqual(Describe(first), Describe(second));
    }

    [Fact]
    public void Describe_StringsAndNulls_KeepTheirUsualForm()
    {
        var prefix = "ab";
        string? missing = null;
        Expression<Func<string, string>> selector = name => prefix + name + missing;

        var description = selector.Body.Describe();

        Assert.Contains("\"ab\"", description);
        Assert.Contains("null", description);
    }

    [Fact]
    public void Describe_ExpressionWithoutConstants_MatchesTheDefaultText()
    {
        Expression<Func<string, int>> selector = name => name.Length;

        Assert.Equal(selector.Body.ToString(), selector.Body.Describe());
    }
}
