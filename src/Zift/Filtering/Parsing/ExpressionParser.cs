using Zift.Filtering.ExpressionBuilding;
using Zift.Filtering.Model;

namespace Zift.Filtering.Parsing;

internal sealed class ExpressionParser
{
    private readonly ExpressionTokenizer _tokenizer;
    private readonly ExpressionBuilderOptions _options;
    private SyntaxToken _currentToken;
    private int _depth;

    public ExpressionParser(ExpressionTokenizer tokenizer, ExpressionBuilderOptions? options = null)
    {
        _tokenizer = tokenizer;
        _options = options ?? new ExpressionBuilderOptions();
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
        return ParseLogical(LogicalOperator.Or, SyntaxTokenType.LogicalOr, ParseAnd);
    }

    private PredicateNode ParseAnd()
    {
        return ParseLogical(LogicalOperator.And, SyntaxTokenType.LogicalAnd, ParseUnary);
    }

    private PredicateNode ParseLogical(
        LogicalOperator op,
        SyntaxTokenType operatorType,
        Func<PredicateNode> parseOperand)
    {
        var first = parseOperand();

        if (_currentToken.Type != operatorType)
        {
            return first;
        }

        var terms = new List<PredicateNode>();
        AddTerm(terms, op, first);

        while (_currentToken.Type == operatorType)
        {
            Consume();
            AddTerm(terms, op, parseOperand());
        }

        return new LogicalNode(op, terms);
    }

    private static void AddTerm(List<PredicateNode> terms, LogicalOperator op, PredicateNode term)
    {
        if (term is LogicalNode logical && logical.Operator == op)
        {
            terms.AddRange(logical.Terms);
        }
        else
        {
            terms.Add(term);
        }
    }

    private PredicateNode ParseUnary()
    {
        if (_currentToken.Type == SyntaxTokenType.LogicalNot)
        {
            Consume();
            EnterNesting();
            var operand = ParseUnary();
            LeaveNesting();

            return new NotNode(operand);
        }

        return ParsePrimaryPredicate();
    }

    private PredicateNode ParsePrimaryPredicate()
    {
        if (_currentToken.Type == SyntaxTokenType.ParenOpen)
        {
            Consume();
            EnterNesting();
            var expr = ParseOr();
            LeaveNesting();
            Expect(SyntaxTokenType.ParenClose);

            return expr;
        }

        var path = ParsePropertyPath();

        if (_currentToken.Type == SyntaxTokenType.Colon)
        {
            if (_tokenizer.PeekToken().Type is SyntaxTokenType.Any or SyntaxTokenType.All)
            {
                return ParseQuantifier(path);
            }

            var projection = ParseProjection(path);

            return ParseComparison(projection);
        }

        return ParseComparison(path);
    }

    private QuantifierNode ParseQuantifier(PropertyPathNode source)
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
            EnterNesting();
            predicate = ParseOr();
            LeaveNesting();
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

    private ProjectionNode ParseProjection(PropertyPathNode source)
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
                if (items.Count >= _options.MaxListItems)
                {
                    throw new SyntaxErrorException(
                        $"List literal exceeds the maximum of {_options.MaxListItems} items.",
                        _currentToken);
                }

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
            SyntaxTokenType.NumberLiteral => new NumberLiteral(token.Text),
            _ => throw new SyntaxErrorException($"Expected literal, found {token.Type}.", token)
        };
    }

    private void EnterNesting()
    {
        if (++_depth > _options.MaxNestingDepth)
        {
            throw new SyntaxErrorException(
                $"Expression nesting exceeds the maximum depth of {_options.MaxNestingDepth}.",
                _currentToken);
        }
    }

    private void LeaveNesting() => _depth--;

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
