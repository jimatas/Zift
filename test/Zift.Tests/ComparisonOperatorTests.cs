namespace Zift.Tests;

using Filtering.Dynamic;

public class ComparisonOperatorTests
{
    [Fact]
    public void Constructor_SetsTypeCorrectly()
    {
        var op = new ComparisonOperator(ComparisonOperatorType.Equal);

        Assert.Equal(ComparisonOperatorType.Equal, op.Type);
        Assert.Empty(op.Modifiers);
    }

    [Fact]
    public void HasModifier_ModifierExists_ReturnsTrue()
    {
        var op = new ComparisonOperator(ComparisonOperatorType.Contains)
        {
            Modifiers = new HashSet<string> { "i" }
        };

        Assert.True(op.HasModifier("i"));
    }

    [Fact]
    public void HasModifier_ModifierDoesNotExist_ReturnsFalse()
    {
        var op = new ComparisonOperator(ComparisonOperatorType.Contains)
        {
            Modifiers = new HashSet<string> { "trim" }
        };

        Assert.False(op.HasModifier("i"));
    }

    [Fact]
    public void ToString_WithoutModifiers_ReturnsTypeOnly()
    {
        var op = new ComparisonOperator(ComparisonOperatorType.NotEqual);

        var result = op.ToString();

        Assert.Equal("!=", result);
    }

    [Fact]
    public void ToString_WithModifiers_ReturnsTypeAndModifiers()
    {
        var op = new ComparisonOperator(ComparisonOperatorType.Contains)
        {
            Modifiers = new HashSet<string> { "i", "trim" }
        };

        var result = op.ToString();

        Assert.StartsWith("%=", result);
        Assert.Contains(":i:", result);
        Assert.EndsWith("trim", result);
    }
}
