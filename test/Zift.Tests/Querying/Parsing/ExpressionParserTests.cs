namespace Zift.Querying.Parsing;

using Model;

public sealed class ExpressionParserTests
{
    public sealed class Comparisons
    {
        [Fact]
        public void Parse_EqualComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a == 1");

            Assert.Equal(ComparisonOperator.Equal, cmp.Operator);
            Assert.IsType<PropertyPathNode>(cmp.Left);
            Assert.IsType<NumberLiteral>(cmp.Right);
        }

        [Fact]
        public void Parse_NotEqualComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a != 1");
            Assert.Equal(ComparisonOperator.NotEqual, cmp.Operator);
        }

        [Fact]
        public void Parse_LessThanComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a < 1");
            Assert.Equal(ComparisonOperator.LessThan, cmp.Operator);
        }

        [Fact]
        public void Parse_LessThanOrEqualComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a <= 1");
            Assert.Equal(ComparisonOperator.LessThanOrEqual, cmp.Operator);
        }

        [Fact]
        public void Parse_GreaterThanComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a > 1");
            Assert.Equal(ComparisonOperator.GreaterThan, cmp.Operator);
        }

        [Fact]
        public void Parse_GreaterThanOrEqualComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a >= 1");
            Assert.Equal(ComparisonOperator.GreaterThanOrEqual, cmp.Operator);
        }

        [Fact]
        public void Parse_ContainsComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a %= \"x\"");

            Assert.Equal(ComparisonOperator.Contains, cmp.Operator);
            Assert.IsType<StringLiteral>(cmp.Right);
        }

        [Fact]
        public void Parse_StartsWithComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a ^= \"x\"");
            Assert.Equal(ComparisonOperator.StartsWith, cmp.Operator);
        }

        [Fact]
        public void Parse_EndsWithComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a $= \"x\"");
            Assert.Equal(ComparisonOperator.EndsWith, cmp.Operator);
        }

        [Fact]
        public void Parse_InComparison_ParsesCorrectOperator()
        {
            var cmp = ParseComparison("a in [1, 2, 3]");

            Assert.Equal(ComparisonOperator.In, cmp.Operator);

            var list = Assert.IsType<ListLiteral>(cmp.Right);
            Assert.Equal(3, list.Items.Count);
        }

        [Fact]
        public void Parse_InComparisonWithMixedLiterals_PreservesItemTypes()
        {
            var cmp = ParseComparison("a in [1, null, \"x\"]");

            var list = Assert.IsType<ListLiteral>(cmp.Right);

            Assert.Collection(
                list.Items,
                item => Assert.IsType<NumberLiteral>(item),
                item => Assert.IsType<NullLiteral>(item),
                item => Assert.IsType<StringLiteral>(item));
        }

        [Fact]
        public void Parse_ComparisonWithNestedPropertyPath_PreservesAllSegments()
        {
            var cmp = ParseComparison("a.b.c == 1");

            var path = Assert.IsType<PropertyPathNode>(cmp.Left);
            Assert.Equal(["a", "b", "c"], path.Segments);
        }
    }

    public sealed class Literals
    {
        [Theory]
        [InlineData("a == 1", typeof(int))]
        [InlineData("a == -1", typeof(int))]
        public void Parse_IntegerLiteral_ReturnsNumberLiteralWithIntValue(string text, Type expectedType)
        {
            var number = Assert.IsType<NumberLiteral>(ParseLiteral(text));
            Assert.IsType(expectedType, number.Value);
        }

        [Theory]
        [InlineData("a == 1.5")]
        [InlineData("a == .5")]
        [InlineData("a == 1e3")]
        [InlineData("a == -1e-3")]
        public void Parse_FloatingPointLiteral_ReturnsNumberLiteralWithDoubleValue(string text)
        {
            var number = Assert.IsType<NumberLiteral>(ParseLiteral(text));
            Assert.IsType<double>(number.Value);
        }

        [Fact]
        public void Parse_BooleanLiteral_ReturnsBooleanLiteral()
        {
            Assert.IsType<BooleanLiteral>(ParseLiteral("a == true"));
            Assert.IsType<BooleanLiteral>(ParseLiteral("a == false"));
        }

        [Fact]
        public void Parse_NullLiteral_ReturnsNullLiteral()
        {
            Assert.IsType<NullLiteral>(ParseLiteral("a == null"));
        }

        [Theory]
        [InlineData("a == \"x\"", "x")]
        [InlineData("a == 'x'", "x")]
        public void Parse_StringLiteral_ReturnsStringLiteralWithValue(string text, string expected)
        {
            var str = Assert.IsType<StringLiteral>(ParseLiteral(text));
            Assert.Equal(expected, str.Value);
        }

        [Fact]
        public void Parse_StringLiteralWithEscapedQuote_PreservesEscapedQuote()
        {
            var str = Assert.IsType<StringLiteral>(
                ParseLiteral("a == \"a \\\" b\""));

            Assert.Equal("a \\\" b", str.Value);
        }

        [Fact]
        public void Parse_EmptyListLiteral_ReturnsListWithNoItems()
        {
            Assert.Empty(
                Assert.IsType<ListLiteral>(ParseLiteral("a in []")).Items);
        }

        [Fact]
        public void Parse_SingleItemListLiteral_ReturnsListWithOneItem()
        {
            Assert.Single(
                Assert.IsType<ListLiteral>(ParseLiteral("a in [1]")).Items);
        }

        [Fact]
        public void Parse_MultiItemListLiteral_ReturnsListWithAllItems()
        {
            Assert.Equal(3,
                Assert.IsType<ListLiteral>(ParseLiteral("a in [1, 2, 3]")).Items.Count);
        }

        [Fact]
        public void Parse_ListLiteralWithMixedItems_PreservesItemTypes()
        {
            var list = Assert.IsType<ListLiteral>(
                ParseLiteral("a in [1, null, \"x\", true]"));

            Assert.Collection(
                list.Items,
                i => Assert.IsType<NumberLiteral>(i),
                i => Assert.IsType<NullLiteral>(i),
                i => Assert.IsType<StringLiteral>(i),
                i => Assert.IsType<BooleanLiteral>(i));
        }
    }

    public sealed class Logical
    {
        [Fact]
        public void Parse_NotExpressionWithSinglePredicate_ReturnsNotNode()
        {
            var not = Assert.IsType<NotNode>(Parse("!a == 1"));
            Assert.IsType<ComparisonNode>(not.Inner);
        }

        [Fact]
        public void Parse_DoubleNotExpression_ReturnsNestedNotNodes()
        {
            var outer = Assert.IsType<NotNode>(Parse("!!a == 1"));
            Assert.IsType<NotNode>(outer.Inner);
        }

        [Fact]
        public void Parse_MixedAndOrExpression_RespectsOperatorPrecedence()
        {
            var expr = Parse("a == 1 || b == 2 && c == 3");

            var or = Assert.IsType<LogicalNode>(expr);
            Assert.Equal(LogicalOperator.Or, or.Operator);

            var and = Assert.IsType<LogicalNode>(or.Terms[1]);
            Assert.Equal(LogicalOperator.And, and.Operator);
        }

        [Fact]
        public void Parse_MixedAndOrExpression_DoesNotFlattenTerms()
        {
            var or = Assert.IsType<LogicalNode>(
                Parse("a == 1 && b == 2 || c == 3 && d == 4"));

            Assert.Equal(2, or.Terms.Count);
        }

        [Fact]
        public void Parse_AndExpressionWithRightGrouping_FlattensTerms()
        {
            var and = Assert.IsType<LogicalNode>(
                Parse("a == 1 && (b == 2 && c == 3)"));

            Assert.Equal(LogicalOperator.And, and.Operator);
            Assert.Equal(3, and.Terms.Count);
        }

        [Fact]
        public void Parse_OrExpressionWithRightGrouping_FlattensTerms()
        {
            var or = Assert.IsType<LogicalNode>(
                Parse("a == 1 || (b == 2 || c == 3)"));

            Assert.Equal(LogicalOperator.Or, or.Operator);
            Assert.Equal(3, or.Terms.Count);
        }

        [Fact]
        public void Parse_AndExpressionWithLeftGrouping_FlattensTerms()
        {
            var and = Assert.IsType<LogicalNode>(
                Parse("(a == 1 && b == 2) && c == 3"));

            Assert.Equal(LogicalOperator.And, and.Operator);
            Assert.Equal(3, and.Terms.Count);
        }

        [Fact]
        public void Parse_OrExpressionWithLeftGrouping_FlattensTerms()
        {
            var or = Assert.IsType<LogicalNode>(
                Parse("(a == 1 || b == 2) || c == 3"));

            Assert.Equal(LogicalOperator.Or, or.Operator);
            Assert.Equal(3, or.Terms.Count);
        }
    }

    public sealed class Grouping
    {
        [Fact]
        public void Parse_GroupedPredicate_ReturnsInnerPredicate()
        {
            Assert.IsType<ComparisonNode>(Parse("(a == 1)"));
        }

        [Fact]
        public void Parse_GroupedExpression_OverridesLogicalPrecedence()
        {
            var and = Assert.IsType<LogicalNode>(
                Parse("(a == 1 || b == 2) && c == 3"));

            var or = Assert.IsType<LogicalNode>(and.Terms[0]);
            Assert.Equal(LogicalOperator.Or, or.Operator);
        }

        [Fact]
        public void Parse_NotExpressionWithGrouping_AppliesToGroupedExpression()
        {
            var not = Assert.IsType<NotNode>(
                Parse("!(a == 1 || b == 2)"));

            Assert.IsType<LogicalNode>(not.Inner);
        }
    }

    public sealed class Projections
    {
        [Fact]
        public void Parse_CountProjection_ParsesCountProjection()
        {
            var cmp = ParseComparison("items:count > 0");

            var projection = Assert.IsType<ProjectionNode>(cmp.Left);
            Assert.Equal(CollectionProjection.Count, projection.Projection);
        }

        [Fact]
        public void Parse_CountProjection_PreservesSourcePropertyPath()
        {
            var projection = Assert.IsType<ProjectionNode>(
                ParseComparison("a.b.c:count >= 1").Left);

            var source = Assert.IsType<PropertyPathNode>(projection.Source);
            Assert.Equal(["a", "b", "c"], source.Segments);
        }

        [Fact]
        public void Parse_ProjectionWithoutComparison_ThrowsSyntaxErrorException()
        {
            Assert.Throws<SyntaxErrorException>(() =>
                Parse("items:count"));
        }

        [Fact]
        public void Parse_UnknownProjection_ThrowsSyntaxErrorException()
        {
            Assert.Throws<SyntaxErrorException>(() =>
                Parse("items:sum > 0"));
        }
    }

    public sealed class Quantifiers
    {
        [Fact]
        public void Parse_AnyQuantifierWithPredicate_ReturnsQuantifierWithPredicate()
        {
            var q = ParseQuantifier("items:any(value > 10)");

            Assert.Equal(QuantifierKind.Any, q.Kind);
            Assert.NotNull(q.Predicate);
        }

        [Fact]
        public void Parse_AnyQuantifierWithoutPredicate_ReturnsQuantifierWithoutPredicate()
        {
            var q = ParseQuantifier("items:any()");

            Assert.Equal(QuantifierKind.Any, q.Kind);
            Assert.Null(q.Predicate);
        }

        [Fact]
        public void Parse_AllQuantifierWithoutPredicate_ThrowsSyntaxErrorException()
        {
            Assert.Throws<SyntaxErrorException>(() =>
                Parse("items:all()"));
        }

        [Fact]
        public void Parse_QuantifierWithNestedPropertyPath_PreservesSourceSegments()
        {
            var q = ParseQuantifier("a.b.c:any(x == 1)");

            var source = Assert.IsType<PropertyPathNode>(q.Source);
            Assert.Equal(["a", "b", "c"], source.Segments);
        }
    }

    public sealed class Errors
    {
        [Fact]
        public void Parse_MissingClosingParen_ThrowsSyntaxErrorException()
        {
            var ex = Assert.Throws<SyntaxErrorException>(() =>
                Parse("(a == 1"));

            Assert.Equal(SyntaxTokenType.End, ex.Token!.Value.Type);
        }

        [Fact]
        public void Parse_UnexpectedClosingParen_ThrowsSyntaxErrorException()
        {
            var ex = Assert.Throws<SyntaxErrorException>(() =>
                Parse("a == 1)"));

            Assert.Equal(SyntaxTokenType.ParenClose, ex.Token!.Value.Type);
        }

        [Fact]
        public void Parse_MissingOperator_ThrowsSyntaxErrorException()
        {
            var ex = Assert.Throws<SyntaxErrorException>(() =>
                Parse("a 1"));

            Assert.Equal(SyntaxTokenType.NumberLiteral, ex.Token!.Value.Type);
        }

        [Fact]
        public void Parse_MissingRightHandExpression_ThrowsSyntaxErrorException()
        {
            var ex = Assert.Throws<SyntaxErrorException>(() =>
                Parse("a =="));

            Assert.Equal(SyntaxTokenType.End, ex.Token!.Value.Type);
        }

        [Fact]
        public void Parse_ListLiteralMissingClosingBracket_ThrowsSyntaxErrorException()
        {
            var ex = Assert.Throws<SyntaxErrorException>(() =>
                Parse("a in [1, 2"));

            Assert.Equal(SyntaxTokenType.End, ex.Token!.Value.Type);
        }
    }

    private static PredicateNode Parse(string text)
    {
        var tokenizer = new ExpressionTokenizer(text);
        var parser = new ExpressionParser(tokenizer);
        return parser.Parse();
    }

    private static ComparisonNode ParseComparison(string text) =>
        Assert.IsType<ComparisonNode>(Parse(text));

    private static LiteralNode ParseLiteral(string text) =>
        ParseComparison(text).Right;

    private static QuantifierNode ParseQuantifier(string text) =>
        Assert.IsType<QuantifierNode>(Parse(text));
}
