namespace Zift.Tests;

using Filtering.Dynamic.Parsing;

public class SyntaxErrorExceptionTests
{
    [Fact]
    public void Constructor_WithToken_SetsToken()
    {
        var token = new SyntaxToken(SyntaxTokenType.Identifier, "Id", 0);
        var ex = new SyntaxErrorException("A parse error occurred.", token);

        Assert.Equal(token, ex.Token);
    }

    [Fact]
    public void Constructor_WithLongTokenValue_TruncatesInMessage()
    {
        var tokenValue = new string('a', SyntaxErrorException.MaxTokenValueLength * 2);
        var token = new SyntaxToken(SyntaxTokenType.Identifier, tokenValue, 0);
        var ex = new SyntaxErrorException("A parse error occurred.", token);

        Assert.Contains("aaa...", ex.Message);
        Assert.DoesNotContain(tokenValue, ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageOnly_TokenIsNull()
    {
        var ex = new SyntaxErrorException("A parse error occurred.");

        Assert.Null(ex.Token);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new SyntaxErrorException("A parse error occurred.");

        Assert.Equal("A parse error occurred.", ex.Message);
    }
}
