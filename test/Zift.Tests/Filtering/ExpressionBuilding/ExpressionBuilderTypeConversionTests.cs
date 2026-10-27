using Zift.Filtering.Model;
using Zift.Fixture;

namespace Zift.Filtering.ExpressionBuilding;

public sealed class ExpressionBuilderTypeConversionTests
{
    [Fact]
    public void Build_IntegerLiteral_ToDecimal_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.DecimalValue),
            ComparisonOperator.GreaterThan,
            new NumberLiteral(5));

        Assert.True(predicate(new TestClass { DecimalValue = 10m }));
        Assert.False(predicate(new TestClass { DecimalValue = 3m }));
    }

    [Fact]
    public void Build_IntegerLiteral_ToDouble_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.DoubleValue),
            ComparisonOperator.GreaterThan,
            new NumberLiteral(5));

        Assert.True(predicate(new TestClass { DoubleValue = 5.5 }));
        Assert.False(predicate(new TestClass { DoubleValue = 4.9 }));
    }

    [Fact]
    public void Build_DoubleLiteral_ToDecimal_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.DecimalValue),
            ComparisonOperator.GreaterThan,
            new NumberLiteral(5.5));

        Assert.True(predicate(new TestClass { DecimalValue = 6m }));
        Assert.False(predicate(new TestClass { DecimalValue = 5m }));
    }

    [Fact]
    public void Build_StringLiteral_ToGuid_EvaluatesCorrectly()
    {
        var id = Guid.NewGuid();

        var predicate = Build(
            nameof(TestClass.GuidValue),
            ComparisonOperator.Equal,
            new StringLiteral(id.ToString()));

        Assert.True(predicate(new TestClass { GuidValue = id }));
        Assert.False(predicate(new TestClass { GuidValue = Guid.NewGuid() }));
    }

    [Fact]
    public void Build_StringLiteral_ToGuid_WithInvalidValue_ThrowsInvalidFilterException()
    {
        Assert.Throws<InvalidFilterException>(() =>
            BuildExpression(
                nameof(TestClass.GuidValue),
                new StringLiteral("not-a-guid")));
    }

    [Fact]
    public void Build_StringLiteral_ToEnum_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.EnumValue),
            ComparisonOperator.Equal,
            new StringLiteral(nameof(TestEnum.B)));

        Assert.True(predicate(new TestClass { EnumValue = TestEnum.B }));
        Assert.False(predicate(new TestClass { EnumValue = TestEnum.D }));
    }

    [Fact]
    public void Build_StringLiteral_ToEnum_WithInvalidValue_ThrowsInvalidFilterException()
    {
        Assert.Throws<InvalidFilterException>(() =>
            BuildExpression(
                nameof(TestClass.EnumValue),
                new StringLiteral("DoesNotExist")));
    }

    [Fact]
    public void Build_StringLiteral_ToDateTime_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.DateTimeValue),
            ComparisonOperator.GreaterThan,
            new StringLiteral("2024-01-01T00:00:00Z"));

        Assert.True(predicate(new TestClass
        {
            DateTimeValue = DateTime.Parse(
                "2024-01-02T00:00:00Z",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind)
        }));
    }

    [Fact]
    public void Build_StringLiteral_ToDateTimeOffset_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.DateTimeOffsetValue),
            ComparisonOperator.GreaterThan,
            new StringLiteral("2024-01-01T00:00:00+00:00"));

        Assert.True(predicate(new TestClass
        {
            DateTimeOffsetValue =
                DateTimeOffset.Parse("2024-01-02T00:00:00+00:00")
        }));
    }

    [Fact]
    public void Build_StringLiteral_ToDateOnly_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.DateOnlyValue),
            ComparisonOperator.Equal,
            new StringLiteral("2024-01-01"));

        Assert.True(predicate(new TestClass
        {
            DateOnlyValue = new DateOnly(2024, 1, 1)
        }));
    }

    [Fact]
    public void Build_StringLiteral_ToTimeOnly_EvaluatesCorrectly()
    {
        var predicate = Build(
            nameof(TestClass.TimeOnlyValue),
            ComparisonOperator.GreaterThan,
            new StringLiteral("10:00"));

        Assert.True(predicate(new TestClass
        {
            TimeOnlyValue = new TimeOnly(11, 0)
        }));
    }

    [Fact]
    public void Build_StringLiteral_ToInt32_ThrowsInvalidFilterException()
    {
        Assert.Throws<InvalidFilterException>(() =>
            BuildExpression(
                nameof(TestClass.Int32Value),
                new StringLiteral("123")));
    }

    [Theory]
    [InlineData(nameof(TestClass.Int32Value), "Int32")]
    [InlineData(nameof(TestClass.StringValue), "String")]
    [InlineData(nameof(TestClass.GuidValue), "Guid")]
    [InlineData(nameof(TestClass.EnumValue), "TestEnum")]
    public void Build_BooleanLiteral_ToNonBooleanProperty_ThrowsInvalidFilterException(
        string property,
        string expectedTypeName)
    {
        var ex = Assert.Throws<InvalidFilterException>(() =>
            BuildExpression(property, new BooleanLiteral(true)));

        Assert.Contains("Boolean", ex.Message);
        Assert.Contains(expectedTypeName, ex.Message);
    }

    [Fact]
    public void Build_BooleanLiteral_ToBooleanProperties_EvaluatesCorrectly()
    {
        var required = Build(
            nameof(TestClass.BooleanValue),
            ComparisonOperator.Equal,
            new BooleanLiteral(true));

        var optional = Build(
            nameof(TestClass.NullableBooleanValue),
            ComparisonOperator.Equal,
            new BooleanLiteral(true));

        Assert.True(required(new TestClass { BooleanValue = true }));
        Assert.False(required(new TestClass { BooleanValue = false }));
        Assert.True(optional(new TestClass { NullableBooleanValue = true }));
        Assert.False(optional(new TestClass { NullableBooleanValue = null }));
    }

    [Fact]
    public void Build_NumberLiteral_ToEnum_ThrowsInvalidFilterException()
    {
        Assert.Throws<InvalidFilterException>(() =>
            BuildExpression(
                nameof(TestClass.EnumValue),
                new NumberLiteral(1)));
    }

    private static Func<TestClass, bool> Build(
        string property,
        ComparisonOperator op,
        LiteralNode literal)
    {
        var builder = CreateBuilder();

        var expr = builder.Build(
            new ComparisonNode(
                new PropertyPathNode([property]),
                op,
                literal));

        return expr.Compile();
    }

    private static void BuildExpression(
        string property,
        LiteralNode literal)
    {
        var builder = CreateBuilder();

        builder.Build(
            new ComparisonNode(
                new PropertyPathNode([property]),
                ComparisonOperator.Equal,
                literal));
    }

    private static ExpressionBuilder<TestClass> CreateBuilder() =>
        new(new ExpressionBuilderOptions
        {
            EnableNullGuards = true,
            ParameterizeValues = false
        });
}
