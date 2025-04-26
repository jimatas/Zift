namespace Zift.Tests;

using Filtering.Dynamic.Parsing;

public class ExpressionTokenizerTests
{
    [Fact]
    public void NextToken_AtEnd_ReturnsEndToken()
    {
        var token = new ExpressionTokenizer("").NextToken();

        Assert.Equal(SyntaxTokenType.End, token.Type);
    }

    [Fact]
    public void NextToken_SubsequentCallsAtEnd_ReturnsEndToken()
    {
        var tokenizer = new ExpressionTokenizer("");
        var tokenType = tokenizer.NextToken().Type;

        var token = tokenizer.NextToken();

        Assert.Equal(SyntaxTokenType.End, tokenType);
        Assert.Equal(SyntaxTokenType.End, token.Type);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData("\r\n\t ")]
    public void NextToken_WithWhitespace_ReturnsWhitespaceToken(string whitespace)
    {
        var token = new ExpressionTokenizer(whitespace).NextToken();

        Assert.Equal(SyntaxTokenType.Whitespace, token.Type);
        Assert.Equal(whitespace, token.Value);
    }

    [Fact]
    public void NextToken_WithParenthesisOpen_ReturnsParenthesisOpenToken()
    {
        var token = new ExpressionTokenizer("(").NextToken();

        Assert.Equal(SyntaxTokenType.ParenthesisOpen, token.Type);
        Assert.Equal("(", token.Value);
    }

    [Fact]
    public void NextToken_WithParenthesisClose_ReturnsParenthesisCloseToken()
    {
        var token = new ExpressionTokenizer(")").NextToken();

        Assert.Equal(SyntaxTokenType.ParenthesisClose, token.Type);
        Assert.Equal(")", token.Value);
    }

    [Theory]
    [InlineData("&&")]
    [InlineData("||")]
    public void NextToken_WithLogicalOperator_ReturnsLogicalOperatorToken(string @operator)
    {
        var token = new ExpressionTokenizer(@operator).NextToken();

        Assert.Equal(SyntaxTokenType.LogicalOperator, token.Type);
        Assert.Equal(@operator, token.Value);
    }

    [Fact]
    public void NextToken_WithUnaryLogicalOperator_ReturnsUnaryLogicalOperatorToken()
    {
        var token = new ExpressionTokenizer("!").NextToken();

        Assert.Equal(SyntaxTokenType.UnaryLogicalOperator, token.Type);
        Assert.Equal("!", token.Value);
    }

    [Theory]
    [InlineData("==")]
    [InlineData("!=")]
    [InlineData("<=")]
    [InlineData("<")]
    [InlineData(">=")]
    [InlineData(">")]
    [InlineData("%=")]
    [InlineData("^=")]
    [InlineData("$=")]
    public void NextToken_WithComparisonOperator_ReturnsComparisonOperatorToken(string @operator)
    {
        var token = new ExpressionTokenizer(@operator).NextToken();

        Assert.Equal(SyntaxTokenType.ComparisonOperator, token.Type);
        Assert.Equal(@operator, token.Value);
    }

    [Fact]
    public void NextToken_WithPropertyName_ReturnsIdentifierToken()
    {
        var token = new ExpressionTokenizer("DatePosted").NextToken();

        Assert.Equal(SyntaxTokenType.Identifier, token.Type);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    public void NextToken_WithKeyword_ReturnsKeywordToken(string keyword)
    {
        var token = new ExpressionTokenizer(keyword).NextToken();

        Assert.Equal(SyntaxTokenType.Keyword, token.Type);
    }

    [Theory]
    [InlineData("42")]
    [InlineData("3.14")]
    [InlineData("-42")]
    [InlineData("-3.14")]
    public void NextToken_WithNumericLiteral_ReturnsNumericLiteralToken(string literal)
    {
        var token = new ExpressionTokenizer(literal).NextToken();

        Assert.Equal(SyntaxTokenType.NumericLiteral, token.Type);
    }

    [Theory]
    [InlineData("\"john.doe@example.com\"")]
    [InlineData("\"\"")]
    [InlineData("'jane_doe@example.com'")]
    [InlineData("''")]
    public void NextToken_WithStringLiteral_ReturnsStringLiteralToken(string literal)
    {
        var token = new ExpressionTokenizer(literal).NextToken();

        Assert.Equal(SyntaxTokenType.StringLiteral, token.Type);
    }

    [Theory]
    [InlineData("~", "~")]
    [InlineData("@", "@")]
    [InlineData("^", "^")]
    [InlineData("*", "*")]
    [InlineData("$", "$")]
    [InlineData("/", "/")]
    [InlineData("+", "+")]
    [InlineData("- ", "-", SyntaxTokenType.Whitespace)]
    [InlineData("%.", "%", SyntaxTokenType.DotSeparator)]
    [InlineData("%-+/$*^@~(", "%-+/$*^@~", SyntaxTokenType.ParenthesisOpen)]
    public void NextToken_WithUnknownToken_ReturnsUnknownToken(
        string expression,
        string unrecognizedSequence,
        SyntaxTokenType nextRecognizedTokenType = SyntaxTokenType.End)
    {
        var tokenizer = new ExpressionTokenizer(expression);
        var unrecognizedToken = tokenizer.NextToken();
        var nextRecognizedToken = tokenizer.NextToken();

        Assert.Equal(SyntaxTokenType.Unknown, unrecognizedToken.Type);
        Assert.Equal(unrecognizedSequence, unrecognizedToken.Value);
        Assert.Equal(nextRecognizedTokenType, nextRecognizedToken.Type);
    }

    [Fact]
    public void NextToken_WithValidTokenFollowingUnknownToken_ReturnsExpectedToken()
    {
        var tokenizer = new ExpressionTokenizer("~null");
        var tokenType = tokenizer.NextToken().Type;

        var token = tokenizer.NextToken();

        Assert.Equal(SyntaxTokenType.Unknown, tokenType);
        Assert.Equal(SyntaxTokenType.Keyword, token.Type);
    }

    [Fact]
    public void PeekToken_WithEmptyExpression_ReturnsEndToken()
    {
        var token = new ExpressionTokenizer("").PeekToken();

        Assert.Equal(SyntaxTokenType.End, token.Type);
    }

    [Fact]
    public void PeekToken_FollowedByPeekToken_ReturnsSameToken()
    {
        var tokenizer = new ExpressionTokenizer("true");

        var peekedToken = tokenizer.PeekToken();
        var peekedTokenAgain = tokenizer.PeekToken();

        Assert.Equal(peekedToken, peekedTokenAgain);
    }

    [Fact]
    public void PeekToken_FollowedByNextToken_ReturnsSameToken()
    {
        var tokenizer = new ExpressionTokenizer("true");

        var peekedToken = tokenizer.PeekToken();
        var nextToken = tokenizer.NextToken();

        Assert.Equal(peekedToken, nextToken);
    }

    [Fact]
    public void NextNonWhitespaceToken_WithWhitespace_ReturnsNonWhitespaceToken()
    {
        var token = new ExpressionTokenizer("\ttrue").NextNonWhitespaceToken();

        Assert.Equal(SyntaxTokenType.Keyword, token.Type);
        Assert.Equal("true", token.Value);
    }

    [Theory]
    [InlineData("42")]
    [InlineData("3.14")]
    [InlineData("-42")]
    [InlineData("-3.14")]
    [InlineData("2147483647")]
    [InlineData("-2147483648")]
    [InlineData("1.7976931348623157E+308")]
    [InlineData("-1.7976931348623157E+308")]
    public void ToNumericValue_WithValidNumericLiteral_ReturnsExpectedValue(string literal)
    {
        var result = new SyntaxToken(SyntaxTokenType.NumericLiteral, literal, 0).ToNumericValue();

        Assert.Equal(literal, string.Format(CultureInfo.InvariantCulture, "{0}", result));
    }

    [Theory]
    [InlineData("42_000")]
    [InlineData("3.14_15")]
    [InlineData("0xFF")]
    public void ToNumericValue_WithInvalidNumericLiteral_ThrowsSyntaxErrorException(string literal)
    {
        var token = new SyntaxToken(SyntaxTokenType.NumericLiteral, literal, 0);

        Assert.Throws<SyntaxErrorException>(() => _ = token.ToNumericValue());
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("null", null)]
    [InlineData("NULL", null)]
    public void ToKeywordValue_WithValidKeyword_ReturnsExpectedValue(string keyword, object? expected)
    {
        var result = new SyntaxToken(SyntaxTokenType.Keyword, keyword, 0).ToKeywordValue();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("nill")]
    [InlineData("void")]
    [InlineData("undefined")]
    public void ToKeywordValue_WithInvalidKeyword_ThrowsSyntaxErrorException(string keyword)
    {
        var token = new SyntaxToken(SyntaxTokenType.Keyword, keyword, 0);

        Assert.Throws<SyntaxErrorException>(() => _ = token.ToKeywordValue());
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("''")]
    [InlineData("\"John Doe\"")]
    [InlineData("'Jane Smith'")]
    public void ToStringValue_WithProperlyQuotedStringLiteral_ReturnsExpectedValue(string literal)
    {
        var result = new SyntaxToken(SyntaxTokenType.StringLiteral, literal, 0).ToStringValue();

        Assert.Equal(literal[1..^1], result);
    }

    [Theory]
    [InlineData("\"")]
    [InlineData("'")]
    [InlineData("\"'")]
    [InlineData("'\"")]
    [InlineData("John Doe")]
    public void ToStringValue_WithImproperlyQuotedStringLiteral_ReturnsStringAsIs(string literal)
    {
        var result = new SyntaxToken(SyntaxTokenType.StringLiteral, literal, 0).ToStringValue();

        Assert.Equal(literal, result);
    }

    [Theory]
    [InlineData("\"\\\"\"", "\"")]
    [InlineData("'\\\"'", "\\\"")]
    [InlineData("\"\\'\"", "\\'")]
    [InlineData("'\\''", "'")]
    public void ToStringValue_WithEscapedStringLiteral_ReturnsExpectedValue(string literal, string expected)
    {
        var result = new SyntaxToken(SyntaxTokenType.StringLiteral, literal, 0).ToStringValue();

        Assert.Equal(expected, result);
    }
}
