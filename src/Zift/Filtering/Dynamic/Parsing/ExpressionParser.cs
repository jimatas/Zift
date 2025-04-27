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
            && token.Type is not SyntaxTokenType.LogicalOperator and not SyntaxTokenType.ParenthesisClose)
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
        var @operator = _tokenizer.NextNonWhitespaceToken().ToComparisonOperator();
        var value = _tokenizer.NextNonWhitespaceToken().ToTypedValue();

        return new(property, @operator, value);
    }

    private PropertyPath ParsePropertyPath(SyntaxToken token)
    {
        var segments = new List<PropertyPathSegment>();
        var segmentBuilder = new PropertyPathSegmentBuilder();

        segmentBuilder.StartNew(token);

        while (true)
        {
            token = _tokenizer.PeekNonWhitespaceToken();

            switch (token.Type)
            {
                case SyntaxTokenType.Colon:
                    _tokenizer.NextToken();
                    token = _tokenizer.NextNonWhitespaceToken();
                    segmentBuilder.ApplyModifier(token);
                    break;

                case SyntaxTokenType.DotSeparator:
                    _tokenizer.NextToken();
                    segments.Add(segmentBuilder.Build(isLastSegment: false, token));

                    token = _tokenizer.NextNonWhitespaceToken();
                    segmentBuilder.StartNew(token);
                    break;

                default:
                    segments.Add(segmentBuilder.Build(isLastSegment: true, token));
                    return new(segments);
            }
        }
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
        if (token.Type is not SyntaxTokenType.UnaryLogicalOperator
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

    private class PropertyPathSegmentBuilder
    {
        private string? _name;
        private QuantifierMode? _quantifier;
        private CollectionProjection? _projection;
        private bool _modifierApplied;

        public void StartNew(SyntaxToken token)
        {
            if (token.Type != SyntaxTokenType.Identifier)
            {
                throw new SyntaxErrorException($"Expected an identifier, but got: {token.Value}", token);
            }

            _name = token.Value;
            _quantifier = null;
            _projection = null;
            _modifierApplied = false;
        }

        public void ApplyModifier(SyntaxToken token)
        {
            if (_modifierApplied)
            {
                throw new SyntaxErrorException("A property segment cannot have more than one modifier.", token);
            }

            switch (token.Type)
            {
                case SyntaxTokenType.QuantifierMode:
                    _quantifier = Enum.Parse<QuantifierMode>(token.Value, ignoreCase: true);
                    break;

                case SyntaxTokenType.CollectionProjection:
                    _projection = Enum.Parse<CollectionProjection>(token.Value, ignoreCase: true);
                    break;

                default:
                    throw new SyntaxErrorException($"Expected a quantifier mode or collection projection, but got: {token.Value}", token);
            }

            _modifierApplied = true;
        }

        public PropertyPathSegment Build(bool isLastSegment, SyntaxToken contextToken)
        {
            var segment = new PropertyPathSegment(_name!)
            {
                Quantifier = _quantifier,
                Projection = _projection
            };

            try
            {
                segment.Validate(isLastSegment);
            }
            catch (InvalidOperationException exception)
            {
                throw new SyntaxErrorException(exception.Message, contextToken);
            }

            return segment;
        }
    }
}
