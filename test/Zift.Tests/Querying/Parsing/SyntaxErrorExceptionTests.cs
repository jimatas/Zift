namespace Zift.Querying.Parsing;

public sealed class SyntaxErrorExceptionTests
{
    [Fact]
    public void Initialize_WithMessage_SetsMessage()
    {
        var message = "Syntax error occurred.";
        
        var ex = new SyntaxErrorException(message);
        
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.Token);
    }

    [Fact]
    public void Initialize_WithMessageAndToken_SetsProperties()
    {
        var message = "Syntax error occurred.";
        var token = new SyntaxToken(
            SyntaxTokenType.Identifier,
            "invalidToken",
            5,
            "invalidToken".Length);

        var ex = new SyntaxErrorException(message, token);

        Assert.StartsWith(message, ex.Message);
        Assert.Contains("Token: Identifier", ex.Message);
        Assert.Contains("Position: 5", ex.Message);
        Assert.Equal(token, ex.Token);
    }

    [Fact]
    public void Initialize_WithLongTokenText_TruncatesTextInMessage()
    {
        var message = "Syntax error occurred.";
        var longTokenText = new string('x', 300);
        var token = new SyntaxToken(
            SyntaxTokenType.Identifier,
            longTokenText,
            10,
            longTokenText.Length);

        var ex = new SyntaxErrorException(message, token);

        Assert.StartsWith(message, ex.Message);
        Assert.Contains("Text:", ex.Message);
        Assert.Contains("...", ex.Message);
        Assert.DoesNotContain(longTokenText, ex.Message);
        Assert.Equal(token, ex.Token);
    }
}
