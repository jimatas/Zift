using Zift.Filtering.ExpressionBuilding;
using Zift.Filtering.Parsing;
using Zift.Fixture;

namespace Zift.Filtering.E2E;

public sealed class ExpressionEndToEndTests
{
    [Fact]
    public void AnyQuantifierWithPredicate_RespectsLogicalPrecedence()
    {
        var predicate = Compile(
            "SubEntities:any(Int32Value > 3 && NullableInt32Value != null)");

        Assert.True(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 2, NullableInt32Value = 5 },
                new TestClass { Int32Value = 4, NullableInt32Value = 1 }
            ]
        }));

        Assert.False(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 4, NullableInt32Value = null }
            ]
        }));
    }

    [Fact]
    public void AllQuantifierWithEmptyOrNullCollection_ReturnsTrue()
    {
        var predicate = Compile(
            "SubEntities:all(Int32Value > 0)");

        Assert.True(predicate(new TestClass { SubEntities = [] }));
        Assert.True(predicate(new TestClass { SubEntities = null! }));
        Assert.False(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass { Int32Value = 0 }
            ]
        }));
    }

    [Fact]
    public void CountProjectionWithNullOrEmptyCollection_ReturnsFalse()
    {
        var predicate = Compile(
            "SubEntities:count > 1");

        Assert.True(predicate(new TestClass
        {
            SubEntities =
            [
                new TestClass(),
                new TestClass()
            ]
        }));

        Assert.False(predicate(new TestClass { SubEntities = [] }));
        Assert.False(predicate(new TestClass { SubEntities = null! }));
    }

    [Fact]
    public void InOperatorWithNullableValues_MatchesNullAndValues()
    {
        var predicate = Compile(
            "NullableInt32Value in [null, 1, 2]");

        Assert.True(predicate(new TestClass { NullableInt32Value = null }));
        Assert.True(predicate(new TestClass { NullableInt32Value = 2 }));
        Assert.False(predicate(new TestClass { NullableInt32Value = 5 }));
    }

    [Fact]
    public void StringOperatorsCombinedWithLogicalOrAndOrdering_EvaluatesCorrectly()
    {
        var predicate = Compile(
            "StringValue ^= \"Al\" || StringValue > \"Bob\"");

        Assert.True(predicate(new TestClass { StringValue = "Alice" }));
        Assert.True(predicate(new TestClass { StringValue = "Charlie" }));
        Assert.False(predicate(new TestClass { StringValue = "Bob" }));
    }

    [Theory]
    [InlineData("StringValue == \"say \\\"hi\\\"\"", "say \"hi\"")]
    [InlineData("StringValue == 'it\\'s'", "it's")]
    [InlineData("StringValue == 'a\\\\b'", "a\\b")]
    public void StringLiteralWithEscapes_MatchesDecodedValue(string text, string value)
    {
        var predicate = Compile(text);

        Assert.True(predicate(new TestClass { StringValue = value }));
        Assert.False(predicate(new TestClass { StringValue = "other" }));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmptyInListInsideQuantifier_MatchesNothingForNullAndNonNullElements(bool enableNullGuards)
    {
        var options = new ExpressionBuilderOptions { EnableNullGuards = enableNullGuards };

        var source = new[]
        {
            new TestClass
            {
                Int32Value = 1,
                SubEntities = [new TestClass { NullableInt32Value = null }]
            },
            new TestClass
            {
                Int32Value = 2,
                SubEntities = [new TestClass { NullableInt32Value = 5 }, new TestClass { NullableInt32Value = null }]
            },
            new TestClass { Int32Value = 3, SubEntities = [] }
        }.AsQueryable();

        int[] Ids(string filter) =>
            source.Where(filter, options).Select(t => t.Int32Value).OrderBy(i => i).ToArray();

        Assert.Empty(Ids("SubEntities:any(NullableInt32Value in [])"));
        Assert.Equal([3], Ids("SubEntities:all(NullableInt32Value in [])"));
        Assert.Equal([1, 2, 3], Ids("!SubEntities:any(NullableInt32Value in [])"));
        Assert.Equal([1, 2], Ids("SubEntities:any(NullableInt32Value in [null])"));
    }

    private static Func<TestClass, bool> Compile(string text)
    {
        var tokenizer = new ExpressionTokenizer(text);
        var parser = new ExpressionParser(tokenizer);
        var predicate = parser.Parse();

        var builder = new ExpressionBuilder<TestClass>(
            new ExpressionBuilderOptions
            {
                EnableNullGuards = true,
                ParameterizeValues = false
            });

        return builder.Build(predicate).Compile();
    }
}
