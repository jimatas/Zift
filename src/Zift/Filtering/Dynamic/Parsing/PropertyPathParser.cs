namespace Zift.Filtering.Dynamic.Parsing;

/// <summary>
/// Parses a dotted property path with optional quantifiers or projections (e.g., <c>Product.Reviews:count</c>).
/// </summary>
/// <param name="tokenizer">The tokenizer providing the input tokens.</param>
internal class PropertyPathParser(ExpressionTokenizer tokenizer)
{
    private readonly ExpressionTokenizer _tokenizer = tokenizer;

    /// <summary>
    /// Parses a property path starting from the specified token.
    /// </summary>
    /// <param name="firstToken">The initial identifier token of the path.</param>
    /// <returns>The parsed <see cref="PropertyPath"/>.</returns>
    /// <exception cref="SyntaxErrorException">Thrown if the path syntax is invalid.</exception>
    public PropertyPath Parse(SyntaxToken firstToken)
    {
        var segments = new List<PropertyPathSegment>();
        var segmentBuilder = new SegmentBuilder();

        segmentBuilder.StartNew(firstToken);

        while (true)
        {
            var token = _tokenizer.PeekNonWhitespaceToken();

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

    private class SegmentBuilder
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

            if (token.Type != SyntaxTokenType.Identifier)
            {
                throw new SyntaxErrorException($"Expected an identifier, but got: {token.Value}", token);
            }

            if (QuantifierModeExtensions.TryParse(token.Value, out var quantifier))
            {
                _quantifier = quantifier;
            }
            else if (CollectionProjectionExtensions.TryParse(token.Value, out var projection))
            {
                _projection = projection;
            }
            else
            {
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
