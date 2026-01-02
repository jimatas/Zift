namespace Zift.Querying.ExpressionBuilding;

using Fixture;
using Model;

public sealed class ExpressionBuilderErrorTests
{
    [Fact]
    public void Build_LogicalNodeWithNoTerms_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(
                new LogicalNode(
                    LogicalOperator.And,
                    [])));
    }

    [Fact]
    public void Build_LogicalNodeWithUnknownOperator_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new LogicalNode(
                    (LogicalOperator)999,
                    [
                        new ComparisonNode(
                        new PropertyPathNode([nameof(TestClass.Int32Value)]),
                        ComparisonOperator.Equal,
                        new NumberLiteral(1))
                    ])));
    }

    [Fact]
    public void Build_NullComparisonWithUnsupportedOperator_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.NullableInt32Value)]),
                    ComparisonOperator.GreaterThan,
                    new NullLiteral())));
    }

    [Fact]
    public void Build_StringComparisonWithUnsupportedOperator_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.StringValue)]),
                    ComparisonOperator.In,
                    new StringLiteral("abc"))));
    }

    [Fact]
    public void Build_StringComparisonWithUnknownOperator_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.StringValue)]),
                    (ComparisonOperator)999,
                    new StringLiteral("abc"))));
    }

    [Fact]
    public void Build_InComparisonWithNonListLiteral_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.In,
                    new NumberLiteral(5))));
    }

    [Fact]
    public void Build_AllQuantifierWithoutPredicate_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(
                new QuantifierNode(
                    new PropertyPathNode([nameof(TestClass.SubEntities)]),
                    QuantifierKind.All,
                    Predicate: null)));
    }

    [Fact]
    public void Build_QuantifierOnNonCollectionProperty_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(
                new QuantifierNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    QuantifierKind.Any,
                    new ComparisonNode(
                        new PropertyPathNode([nameof(TestClass.Int32Value)]),
                        ComparisonOperator.Equal,
                        new NumberLiteral(5)))));
    }

    [Fact]
    public void Build_ProjectionOnNonCollectionProperty_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(
                new ComparisonNode(
                    new ProjectionNode(
                        new PropertyPathNode([nameof(TestClass.Int32Value)]),
                        CollectionProjection.Count),
                    ComparisonOperator.Equal,
                    new NumberLiteral(0))));
    }

    [Fact]
    public void Build_ProjectionWithUnknownProjection_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new ProjectionNode(
                        new PropertyPathNode([nameof(TestClass.SubEntities)]),
                        (CollectionProjection)999),
                    ComparisonOperator.Equal,
                    new NumberLiteral(0))));
    }

    [Fact]
    public void Build_UnsupportedPredicateNode_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(new UnsupportedPredicateNode()));
    }

    [Fact]
    public void Build_UnsupportedPropertyNode_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new UnsupportedPropertyNode(),
                    ComparisonOperator.Equal,
                    new NumberLiteral(1))));
    }

    [Fact]
    public void Build_UnsupportedLiteral_ThrowsNotSupportedException()
    {
        var builder = CreateBuilder();

        Assert.Throws<NotSupportedException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode([nameof(TestClass.Int32Value)]),
                    ComparisonOperator.Equal,
                    new UnsupportedLiteral())));
    }

    [Fact]
    public void Build_PropertyPathWithUnknownSegment_ThrowsArgumentException()
    {
        var builder = new ExpressionBuilder<TestClass>(
            new ExpressionBuilderOptions
            {
                EnableNullGuards = true
            });

        Assert.Throws<ArgumentException>(() =>
            builder.Build(
                new ComparisonNode(
                    new PropertyPathNode(
                    [
                        nameof(TestClass.NestedEntity),
                        "DoesNotExist"
                    ]),
                    ComparisonOperator.Equal,
                    new NumberLiteral(1))));
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = true,
            ParameterizeValues = false
        });

    private sealed record UnsupportedPredicateNode : PredicateNode;
    private sealed record UnsupportedPropertyNode : PropertyNode;
    private sealed record UnsupportedLiteral : LiteralNode;
}
