using Zift.Filtering.Parsing;

namespace Zift.Filtering;

public sealed class InvalidFilterExceptionTests
{
    [Fact]
    public void Initialize_WithMessage_SetsMessageWithoutPosition()
    {
        var ex = new InvalidFilterException("Bad filter.");

        Assert.Equal("Bad filter.", ex.Message);
        Assert.Null(ex.Position);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Initialize_WithMessageAndInnerException_KeepsBoth()
    {
        var inner = new FormatException("inner");

        var ex = new InvalidFilterException("Bad filter.", inner);

        Assert.Equal("Bad filter.", ex.Message);
        Assert.Same(inner, ex.InnerException);
        Assert.Null(ex.Position);
    }

    [Fact]
    public void Initialize_WithMessageAndPosition_SetsPosition()
    {
        var ex = new InvalidFilterException("Bad filter.", 7);

        Assert.Equal("Bad filter.", ex.Message);
        Assert.Equal(7, ex.Position);
    }

    [Fact]
    public void InvalidFilterException_IsAFormatException()
    {
        Assert.IsAssignableFrom<FormatException>(new InvalidFilterException("Bad filter."));
    }

    [Fact]
    public void SyntaxErrorException_IsAnInvalidFilterException()
    {
        Assert.IsAssignableFrom<InvalidFilterException>(new SyntaxErrorException("Bad syntax."));
    }

    [Fact]
    public void LexicalErrorException_IsAnInvalidFilterException()
    {
        Assert.IsAssignableFrom<InvalidFilterException>(new LexicalErrorException("Bad character.", 3));
    }

    [Fact]
    public void SyntaxErrorException_WithoutToken_HasNoPosition()
    {
        var ex = new SyntaxErrorException("Bad syntax.");

        Assert.Null(ex.Position);
    }

    [Fact]
    public void SyntaxErrorException_WithToken_ExposesTheTokenPosition()
    {
        var token = new SyntaxToken(SyntaxTokenType.Identifier, "abc", 5, 3);

        var ex = new SyntaxErrorException("Bad syntax.", token);

        Assert.Equal(5, ex.Position);
    }

    [Fact]
    public void LexicalErrorException_ExposesThePosition()
    {
        var ex = new LexicalErrorException("Bad character.", 3);

        Assert.Equal(3, ex.Position);
    }
}
