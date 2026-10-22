namespace Zift.Querying.Parsing;

public sealed class ExpressionTokenizerTests
{
    public sealed class Errors
    {
        [Fact]
        public void Tokenize_SingleAmpersand_ThrowsLexicalErrorException()
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize("a == 1 & b == 2"));

            Assert.Contains("&&", ex.Message);
        }

        [Fact]
        public void Tokenize_SinglePipe_ThrowsLexicalErrorException()
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize("a == 1 | b == 2"));

            Assert.Contains("||", ex.Message);
        }

        [Fact]
        public void Tokenize_SingleEquals_ThrowsLexicalErrorException()
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize("a = 1"));

            Assert.Contains("==", ex.Message);
        }

        [Theory]
        [InlineData("%", "%=")]
        [InlineData("^", "^=")]
        [InlineData("$", "$=")]
        public void Tokenize_InvalidStringOperator_ThrowsLexicalErrorException(string input, string hint)
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize(input));

            Assert.Contains(hint, ex.Message);
        }

        [Theory]
        [InlineData("&", "&&")]
        [InlineData("|", "||")]
        [InlineData("=", "==")]
        public void Tokenize_InvalidLogicalOrEqualityOperator_ThrowsLexicalErrorException(string input, string hint)
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize(input));

            Assert.Contains(hint, ex.Message);
        }

        [Fact]
        public void Tokenize_InvalidNumericLiteral_ThrowsLexicalErrorException()
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize("a == 1e"));

            Assert.Contains("Invalid numeric literal", ex.Message);
        }

        [Theory]
        [InlineData("1.")]
        [InlineData("1e")]
        [InlineData("1.e2")]
        [InlineData("1e+")]
        [InlineData("1e-")]
        [InlineData("+.")]
        [InlineData("-.")]
        [InlineData("+")]
        [InlineData("++1")]
        [InlineData("-")]
        [InlineData("--1")]
        public void Tokenize_MalformedNumericLiteral_ThrowsLexicalErrorException(string input)
        {
            Assert.Throws<LexicalErrorException>(() =>
                Tokenize(input));
        }

        [Fact]
        public void Tokenize_UnterminatedStringLiteral_ThrowsLexicalErrorException()
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize("\"abc"));

            Assert.Contains("Unterminated", ex.Message);
        }

        [Fact]
        public void Tokenize_UnexpectedCharacter_ThrowsLexicalErrorException()
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize("a == @"));

            Assert.Contains("Unexpected character", ex.Message);
        }
    }

    public sealed class IdentifiersAndKeywords
    {
        [Fact]
        public void Tokenize_Identifier_ReturnsIdentifierToken()
        {
            var tokens = Tokenize("a abc a1 _a a_b");

            Assert.Equal(
                [
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }

        [Fact]
        public void Tokenize_Keyword_ReturnsKeywordToken()
        {
            var tokens = Tokenize("true false null in any all");

            Assert.Equal(
                [
                    SyntaxTokenType.True,
                    SyntaxTokenType.False,
                    SyntaxTokenType.Null,
                    SyntaxTokenType.In,
                    SyntaxTokenType.Any,
                    SyntaxTokenType.All,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }

        [Fact]
        public void Tokenize_KeywordWithDifferentCasing_TreatedAsIdentifier()
        {
            var tokens = Tokenize("True FALSE Null");

            Assert.Equal(
                [
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }
    }

    public sealed class NumericLiterals
    {
        [Theory]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("-1")]
        [InlineData("+1")]
        [InlineData("0.5")]
        [InlineData(".5")]
        [InlineData("-.5")]
        [InlineData("+.5")]
        [InlineData("1.5")]
        [InlineData("1e3")]
        [InlineData("1E-3")]
        [InlineData("1e+10")]
        [InlineData("1e-10")]
        public void Tokenize_ValidNumericLiteral_ReturnsNumberLiteralToken(string literal)
        {
            var tokens = Tokenize(literal);

            Assert.Equal(
                [
                    SyntaxTokenType.NumberLiteral,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }
    }

    public sealed class StringLiterals
    {
        [Fact]
        public void Tokenize_EmptyStringLiteral_ReturnsEmptyStringToken()
        {
            var tokens = Tokenize("\"\"");

            var token = Assert.Single(tokens,
                t => t.Type == SyntaxTokenType.StringLiteral);

            Assert.Equal(string.Empty, token.Text);
        }

        [Theory]
        [InlineData("\"a b\"")]
        [InlineData("'a b'")]
        public void Tokenize_QuotedStringLiteral_ReturnsStringLiteralToken(string input)
        {
            var tokens = Tokenize(input);

            Assert.Equal(
                [
                    SyntaxTokenType.StringLiteral,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }

        [Fact]
        public void Tokenize_StringLiteralWithoutEscapes_PreservesText()
        {
            var tokens = Tokenize("\"abc\"");

            var token = Assert.Single(tokens,
                t => t.Type == SyntaxTokenType.StringLiteral);

            Assert.Equal("abc", token.Text);
        }

        [Theory]
        [InlineData("\"a\\\"b\"", "a\"b")]
        [InlineData("'a\\'b'", "a'b")]
        [InlineData("\"a\\\\b\"", "a\\b")]
        [InlineData("'a\\\\'", "a\\")]
        [InlineData("\"it's\"", "it's")]
        [InlineData("'say \"hi\"'", "say \"hi\"")]
        public void Tokenize_StringLiteralWithEscapes_DecodesEscapes(string input, string expected)
        {
            var tokens = Tokenize(input);

            var token = Assert.Single(tokens,
                t => t.Type == SyntaxTokenType.StringLiteral);

            Assert.Equal(expected, token.Text);
            Assert.Equal(input.Length, token.Length);
        }

        [Fact]
        public void PeekToken_StringLiteralWithEscapes_AdvancesPastSourceLengthOnNextToken()
        {
            var tokenizer = new ExpressionTokenizer("'a\\\\b' == 'c\\'d'");

            var peeked = tokenizer.PeekToken();
            var first = tokenizer.NextToken();
            var op = tokenizer.NextToken();
            var second = tokenizer.NextToken();

            Assert.Equal(peeked, first);
            Assert.Equal("a\\b", first.Text);
            Assert.Equal(SyntaxTokenType.Equal, op.Type);
            Assert.Equal("c'd", second.Text);
            Assert.Equal(SyntaxTokenType.End, tokenizer.NextToken().Type);
        }

        [Theory]
        [InlineData("'C:\\temp'", 3)]
        [InlineData("\"a\\nb\"", 2)]
        [InlineData("'\\x'", 1)]
        public void Tokenize_UnsupportedEscapeSequence_ThrowsLexicalErrorException(string input, int position)
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize(input));

            Assert.Contains("Invalid escape sequence", ex.Message);
            Assert.Equal(position, ex.Position);
        }

        [Theory]
        [InlineData("\"abc\\")]
        [InlineData("'abc\\'")]
        [InlineData("\"abc\\\"")]
        public void Tokenize_EscapeAtEndOfInput_ThrowsUnterminatedStringLiteral(string input)
        {
            var ex = Assert.Throws<LexicalErrorException>(() =>
                Tokenize(input));

            Assert.Contains("Unterminated", ex.Message);
            Assert.Equal(0, ex.Position);
        }
    }

    public sealed class Operators
    {
        [Fact]
        public void Tokenize_LogicalOperator_ReturnsCorrectLogicalToken()
        {
            var tokens = Tokenize("a && b || !c");

            Assert.Equal(
                [
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.LogicalAnd,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.LogicalOr,
                    SyntaxTokenType.LogicalNot,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }

        [Fact]
        public void Tokenize_ComparisonOperator_ReturnsCorrectComparisonToken()
        {
            var tokens = Tokenize("a == b != c < d <= e > f >= g");

            Assert.Equal(
                [
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Equal,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.NotEqual,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.LessThan,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.LessThanOrEqual,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.GreaterThan,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.GreaterThanOrEqual,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }

        [Fact]
        public void Tokenize_StringOperator_ReturnsCorrectStringOperatorToken()
        {
            var tokens = Tokenize("a %= b ^= c $= d");

            Assert.Equal(
                [
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Contains,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.StartsWith,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.EndsWith,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }
    }

    public sealed class Punctuation
    {
        [Fact]
        public void Tokenize_PunctuationCharacter_ReturnsCorrectPunctuationToken()
        {
            var tokens = Tokenize("a.b:c,(d)[e]");

            Assert.Equal(
                [
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Dot,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Colon,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.Comma,
                    SyntaxTokenType.ParenOpen,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.ParenClose,
                    SyntaxTokenType.BracketOpen,
                    SyntaxTokenType.Identifier,
                    SyntaxTokenType.BracketClose,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }

        [Fact]
        public void Tokenize_SingleDot_ReturnsDotToken()
        {
            var tokens = Tokenize(".");

            Assert.Equal(
                [
                    SyntaxTokenType.Dot,
                    SyntaxTokenType.End
                ],
                TokenTypes(tokens));
        }
    }

    private static IReadOnlyList<SyntaxToken> Tokenize(string text)
    {
        var tokenizer = new ExpressionTokenizer(text);
        var tokens = new List<SyntaxToken>();

        while (true)
        {
            var token = tokenizer.NextToken();
            tokens.Add(token);

            if (token.Type == SyntaxTokenType.End)
            {
                return tokens;
            }
        }
    }

    private static IReadOnlyList<SyntaxTokenType> TokenTypes(IEnumerable<SyntaxToken> tokens) =>
        tokens.Select(t => t.Type).ToArray();
}
