namespace Zift.Querying.Parsing;

using Model;

internal sealed class ExpressionParser
{
    private readonly ExpressionTokenizer _tokenizer;
    private SyntaxToken _currentToken;

    public ExpressionParser(ExpressionTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
        _currentToken = _tokenizer.NextToken();
    }

    public PredicateNode Parse()
    {
        var expr = ParseOr();
        Expect(SyntaxTokenType.End);

        return expr;
    }

    private PredicateNode ParseOr()
    {
        var left = ParseAnd();

        while (_currentToken.Type == SyntaxTokenType.LogicalOr)
        {
            Consume();
            var right = ParseAnd();
            left = MergeLogical(LogicalOperator.Or, left, right);
        }

        return left;
    }

    private PredicateNode ParseAnd()
    {
        var left = ParseUnary();

        while (_currentToken.Type == SyntaxTokenType.LogicalAnd)
        {
            Consume();
            var right = ParseUnary();
            left = MergeLogical(LogicalOperator.And, left, right);
        }

        return left;
    }

    private static LogicalNode MergeLogical(
        LogicalOperator op,
        PredicateNode left,
        PredicateNode right)
    {
        if (left is LogicalNode l && l.Operator == op)
        {
            return new LogicalNode(op, [.. l.Terms, right]);
        }

        if (right is LogicalNode r && r.Operator == op)
        {
            return new LogicalNode(op, [left, .. r.Terms]);
        }

        return new LogicalNode(op, [left, right]);
    }

    private PredicateNode ParseUnary()
    {
        if (_currentToken.Type == SyntaxTokenType.LogicalNot)
        {
            Consume();

            return new NotNode(ParseUnary());
        }

        return ParsePrimaryPredicate();
    }

    private PredicateNode ParsePrimaryPredicate()
    {
        if (_currentToken.Type == SyntaxTokenType.ParenOpen)
        {
            Consume();
            var expr = ParseOr();
            Expect(SyntaxTokenType.ParenClose);

            return expr;
        }

        var path = ParsePropertyPath();

        if (_currentToken.Type == SyntaxTokenType.Colon)
        {
            if (_tokenizer.PeekToken().Type is
                SyntaxTokenType.Any or
                SyntaxTokenType.All)
            {
                return ParseQuantifier(path);
            }

            var projection = ParseProjection(path);

            return ParseComparison(projection);
        }

        return ParseComparison(path);
    }

    private QuantifierNode ParseQuantifier(PropertyNode source)
    {
        Expect(SyntaxTokenType.Colon);

        var kind = _currentToken.Type == SyntaxTokenType.Any
            ? QuantifierKind.Any
            : QuantifierKind.All;

        Consume();
        Expect(SyntaxTokenType.ParenOpen);

        PredicateNode? predicate = null;

        if (_currentToken.Type != SyntaxTokenType.ParenClose)
        {
            predicate = ParseOr();
        }

        var closingParenToken = Expect(SyntaxTokenType.ParenClose);

        if (kind == QuantifierKind.All && predicate is null)
        {
            throw new SyntaxErrorException(
                "Quantifier 'all' requires a predicate.",
                closingParenToken);
        }

        return new QuantifierNode(source, kind, predicate);
    }

    private ComparisonNode ParseComparison(PropertyNode left)
    {
        var op = ParseComparisonOperator();
        var right = ParseValue();

        return new ComparisonNode(left, op, right);
    }

    private ComparisonOperator ParseComparisonOperator()
    {
        var token = Consume();

        return token.Type switch
        {
            SyntaxTokenType.Equal => ComparisonOperator.Equal,
            SyntaxTokenType.NotEqual => ComparisonOperator.NotEqual,
            SyntaxTokenType.LessThan => ComparisonOperator.LessThan,
            SyntaxTokenType.LessThanOrEqual => ComparisonOperator.LessThanOrEqual,
            SyntaxTokenType.GreaterThan => ComparisonOperator.GreaterThan,
            SyntaxTokenType.GreaterThanOrEqual => ComparisonOperator.GreaterThanOrEqual,
            SyntaxTokenType.Contains => ComparisonOperator.Contains,
            SyntaxTokenType.StartsWith => ComparisonOperator.StartsWith,
            SyntaxTokenType.EndsWith => ComparisonOperator.EndsWith,
            SyntaxTokenType.In => ComparisonOperator.In,
            _ => throw new SyntaxErrorException(
                $"Expected comparison operator, found {token.Type}.",
                token)
        };
    }

    private PropertyPathNode ParsePropertyPath()
    {
        var segments = new List<string>
        {
            Expect(SyntaxTokenType.Identifier).Text
        };

        while (_currentToken.Type == SyntaxTokenType.Dot)
        {
            Consume();
            segments.Add(Expect(SyntaxTokenType.Identifier).Text);
        }

        return new PropertyPathNode(segments);
    }

    private ProjectionNode ParseProjection(PropertyNode source)
    {
        Expect(SyntaxTokenType.Colon);

        var token = Expect(SyntaxTokenType.Identifier);

        var projection = token.Text switch
        {
            "count" => CollectionProjection.Count,
            _ => throw new SyntaxErrorException($"Unknown projection '{token.Text}'.", token)
        };

        return new ProjectionNode(source, projection);
    }

    private LiteralNode ParseValue()
    {
        if (_currentToken.Type == SyntaxTokenType.BracketOpen)
        {
            return ParseListLiteral();
        }

        return ParseLiteral();
    }

    private ListLiteral ParseListLiteral()
    {
        Expect(SyntaxTokenType.BracketOpen);

        var items = new List<LiteralNode>();

        if (_currentToken.Type != SyntaxTokenType.BracketClose)
        {
            do
            {
                items.Add(ParseLiteral());
            }
            while (Match(SyntaxTokenType.Comma));
        }

        Expect(SyntaxTokenType.BracketClose);

        return new ListLiteral(items);
    }

    private LiteralNode ParseLiteral()
    {
        var token = Consume();

        return token.Type switch
        {
            SyntaxTokenType.StringLiteral => new StringLiteral(token.Text),
            SyntaxTokenType.True => new BooleanLiteral(true),
            SyntaxTokenType.False => new BooleanLiteral(false),
            SyntaxTokenType.Null => new NullLiteral(),
            SyntaxTokenType.NumberLiteral => ParseNumber(token),
            _ => throw new SyntaxErrorException($"Expected literal, found {token.Type}.", token)
        };
    }

    private static NumberLiteral ParseNumber(SyntaxToken token) =>
        token.Text.Contains('.') ||
        token.Text.Contains('e', StringComparison.OrdinalIgnoreCase)
            ? new NumberLiteral(double.Parse(token.Text, CultureInfo.InvariantCulture))
            : new NumberLiteral(int.Parse(token.Text, CultureInfo.InvariantCulture));

    private SyntaxToken Consume()
    {
        var token = _currentToken;
        _currentToken = _tokenizer.NextToken();

        return token;
    }

    private SyntaxToken Expect(SyntaxTokenType type)
    {
        if (_currentToken.Type != type)
        {
            throw new SyntaxErrorException(
                $"Expected {type}, found {_currentToken.Type}.",
                _currentToken);
        }

        return Consume();
    }

    private bool Match(SyntaxTokenType type)
    {
        if (_currentToken.Type != type)
        {
            return false;
        }

        Consume();

        return true;
    }
}
