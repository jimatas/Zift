namespace Zift.Filtering.Dynamic.Parsing;

public class ExpressionParser(ExpressionTokenizer tokenizer)
{
    private readonly ExpressionTokenizer _tokenizer = tokenizer;

    public FilterGroup Parse()
    {
        var rootGroup = ParseGroup();
        if (rootGroup.Terms.Count == 0)
        {
            throw new SyntaxErrorException("Empty expression detected: An expression must contain at least one term.");
        }

        return rootGroup;
    }

    private FilterGroup ParseGroup(int nestingLevel = 0)
    {
        var groupBuilder = new FilterGroupBuilder();
        var expectLogicalOperator = false;

        for (var token = _tokenizer.NextNonWhitespaceToken();
            token.Type != SyntaxTokenType.End;
            token = _tokenizer.NextNonWhitespaceToken())
        {
            ValidateTokenExpectation(expectLogicalOperator, token);

            var negateNextTerm = false;
            if (token.Type == SyntaxTokenType.UnaryLogicalOperator)
            {
                token = ProcessUnaryLogicalOperator();
                negateNextTerm = true;
            }

            switch (token.Type)
            {
                case SyntaxTokenType.ParenthesisOpen:
                case SyntaxTokenType.Identifier:
                    ProcessTerm(token, groupBuilder, nestingLevel, negateNextTerm);
                    expectLogicalOperator = true;
                    break;

                case SyntaxTokenType.ParenthesisClose:
                    return FinalizeGroup(nestingLevel, groupBuilder, token);

                case SyntaxTokenType.LogicalOperator:
                    ProcessLogicalOperator(token, groupBuilder, expectLogicalOperator);
                    expectLogicalOperator = false;
                    break;

                default:
                    throw new SyntaxErrorException("Unexpected token type encountered.", token);
            }
        }

        if (nestingLevel > 0)
        {
            throw new SyntaxErrorException("Mismatched parentheses: Missing closing parenthesis.");
        }

        return groupBuilder.Build();
    }

    private static void ValidateTokenExpectation(bool expectLogicalOperator, SyntaxToken token)
    {
        if (expectLogicalOperator
            && token.Type is
                not SyntaxTokenType.LogicalOperator
                and not SyntaxTokenType.ParenthesisClose)
        {
            throw new SyntaxErrorException("Expected a logical operator between terms.", token);
        }
    }

    private SyntaxToken ProcessUnaryLogicalOperator()
    {
        var nextToken = _tokenizer.NextNonWhitespaceToken();
        if (nextToken.Type != SyntaxTokenType.ParenthesisOpen)
        {
            throw new SyntaxErrorException("Unary logical operator must be followed by an opening parenthesis.", nextToken);
        }

        return nextToken;
    }

    private void ProcessTerm(SyntaxToken token, FilterGroupBuilder groupBuilder, int nestingLevel, bool negateTerm)
    {
        FilterTerm term = token.Type == SyntaxTokenType.ParenthesisOpen
            ? ParseGroup(nestingLevel + 1)
            : ParseCondition(token);

        if (negateTerm)
        {
            term = term.Negate();
        }

        groupBuilder.AddTerm(term);
    }

    private FilterCondition ParseCondition(SyntaxToken token)
    {
        var property = ParsePropertyPath(token);
        var @operator = ParseComparisonOperator();

        if (@operator.Type == ComparisonOperatorType.In)
        {
            token = _tokenizer.PeekNonWhitespaceToken();
            if (token.Type != SyntaxTokenType.BracketOpen)
            {
                throw new SyntaxErrorException($"Expected an opening bracket, but got: {token.Value}", token);
            }

            var values = ParseValueList();

            return new(property, @operator, values);
        }

        var value = ParseValue();

        return new(property, @operator, value);
    }

    private PropertyPath ParsePropertyPath(SyntaxToken token)
    {
        return new PropertyPathParser(_tokenizer).Parse(token);
    }

    private ComparisonOperator ParseComparisonOperator()
    {
        return new ComparisonOperatorParser(_tokenizer).Parse();
    }

    private object? ParseValue()
    {
        var token = _tokenizer.NextNonWhitespaceToken();

        return token.ToTypedValue();
    }

    private IReadOnlyList<object?> ParseValueList()
    {
        _tokenizer.NextNonWhitespaceToken();

        var values = new List<object?>();
        var expectingValue = true;

        while (true)
        {
            var token = _tokenizer.PeekNonWhitespaceToken();

            if (token.Type == SyntaxTokenType.BracketClose)
            {
                if (expectingValue && values.Count > 0)
                {
                    throw new SyntaxErrorException("Unexpected closing bracket: Expected a value before the closing bracket.", token);
                }

                _tokenizer.NextNonWhitespaceToken();
                break;
            }

            if (!expectingValue)
            {
                if (token.Type != SyntaxTokenType.Comma)
                {
                    throw new SyntaxErrorException($"Expected a comma between values, but got: {token.Value}", token);
                }

                _tokenizer.NextNonWhitespaceToken();
                expectingValue = true;
                continue;
            }

            var value = ParseValue();
            values.Add(value);
            expectingValue = false;
        }

        return values;
    }

    private static FilterGroup FinalizeGroup(int nestingLevel, FilterGroupBuilder groupBuilder, SyntaxToken token)
    {
        if (nestingLevel == 0)
        {
            throw new SyntaxErrorException("Mismatched parentheses: Unexpected closing parenthesis.", token);
        }

        if (groupBuilder.IsEmpty)
        {
            throw new SyntaxErrorException("Empty group detected: A group must contain at least one term.", token);
        }

        return groupBuilder.Build();
    }

    private void ProcessLogicalOperator(SyntaxToken token, FilterGroupBuilder groupBuilder, bool expectLogicalOperator)
    {
        if (groupBuilder.IsEmpty || !expectLogicalOperator)
        {
            throw new SyntaxErrorException("Unexpected logical operator.", token);
        }

        var newOperator = token.ToLogicalOperator();
        groupBuilder.SetOperator(newOperator);

        token = _tokenizer.PeekNonWhitespaceToken();
        if (token.Type is
            not SyntaxTokenType.UnaryLogicalOperator
            and not SyntaxTokenType.ParenthesisOpen
            and not SyntaxTokenType.Identifier)
        {
            throw new SyntaxErrorException("Expected a term after logical operator.", token);
        }
    }

    private class FilterGroupBuilder
    {
        private LogicalOperator? _operator;
        private readonly List<FilterTerm> _terms = new();

        public bool IsEmpty => _terms.Count == 0;

        public void AddTerm(FilterTerm term) => _terms.Add(term);
        public void SetOperator(LogicalOperator newOperator)
        {
            if (_operator.HasValue && _operator.Value != newOperator)
            {
                var completedGroup = Build();
                _terms.Clear();
                _terms.Add(completedGroup);
            }

            _operator = newOperator;
        }

        public FilterGroup Build()
        {
            if (_terms.Count == 1 && _terms[0] is FilterGroup group && !_operator.HasValue)
            {
                return group;
            }

            group = new FilterGroup(_operator ?? LogicalOperator.And);
            _terms.ForEach(group.Terms.Add);

            return group;
        }
    }
}
