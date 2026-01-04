namespace Zift.Pagination.Cursor;

using Execution;
using Fixture;
using Ordering;

public sealed class ComparisonBuilderTests
{
    [Fact]
    public void Comparison_Int32_Ascending_OrdersGreaterValuesAfter()
    {
        var sample = new TestClass { Int32Value = 10 };
        var predicate = Build(s => s.Int32Value, 5, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_Int32_Descending_OrdersSmallerValuesAfter()
    {
        var sample = new TestClass { Int32Value = 3 };
        var predicate = Build(s => s.Int32Value, 5, OrderingDirection.Descending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableInt32_WithValue_OrdersByValue()
    {
        var sample = new TestClass { NullableInt32Value = 10 };
        var predicate = Build(s => s.NullableInt32Value, 1, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableInt32_NullProperty_Ascending_SortsLast()
    {
        var sample = new TestClass { NullableInt32Value = null };
        var predicate = Build(s => s.NullableInt32Value, 5, OrderingDirection.Ascending);
        Assert.False(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableInt32_NullProperty_Descending_SortsFirst()
    {
        var sample = new TestClass { NullableInt32Value = null };
        var predicate = Build(s => s.NullableInt32Value, 5, OrderingDirection.Descending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableInt32_NullCursor_Ascending_AllowsNonNullValues()
    {
        var sample = new TestClass { NullableInt32Value = 10 };
        var predicate = Build(s => s.NullableInt32Value, null, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableInt32_NullCursor_Ascending_RejectsNullValues()
    {
        var sample = new TestClass { NullableInt32Value = null };
        var predicate = Build(s => s.NullableInt32Value, null, OrderingDirection.Ascending);
        Assert.False(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableInt32_NullCursor_Descending_RejectsAllValues()
    {
        var sample = new TestClass { NullableInt32Value = 42 };
        var predicate = Build(s => s.NullableInt32Value, null, OrderingDirection.Descending);
        Assert.False(predicate(sample));
    }

    [Fact]
    public void Comparison_Double_Ascending_OrdersByValue()
    {
        var sample = new TestClass { DoubleValue = 10.5 };
        var predicate = Build(s => s.DoubleValue, 2.1, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableDouble_NullProperty_Ascending_SortsLast()
    {
        var sample = new TestClass { NullableDoubleValue = null };
        var predicate = Build(s => s.NullableDoubleValue, 1.0, OrderingDirection.Ascending);
        Assert.False(predicate(sample));
    }

    [Fact]
    public void Comparison_Decimal_Descending_OrdersByValue()
    {
        var sample = new TestClass { DecimalValue = 5.0m };
        var predicate = Build(s => s.DecimalValue, 10.0m, OrderingDirection.Descending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_String_Ascending_OrdersLexicographically()
    {
        var sample = new TestClass { StringValue = "beta" };
        var predicate = Build(s => s.StringValue, "alpha", OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_String_Descending_OrdersLexicographically()
    {
        var sample = new TestClass { StringValue = "alpha" };
        var predicate = Build(s => s.StringValue, "beta", OrderingDirection.Descending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_Guid_Ascending_OrdersByValue()
    {
        var smaller = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var larger = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var sample = new TestClass { GuidValue = larger };
        var predicate = Build(s => s.GuidValue, smaller, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableGuid_NullProperty_Descending_SortsFirst()
    {
        var sample = new TestClass { NullableGuidValue = null };
        var predicate = Build(s => s.NullableGuidValue, Guid.NewGuid(), OrderingDirection.Descending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_Boolean_Ascending_OrdersFalseBeforeTrue()
    {
        var sample = new TestClass { BooleanValue = true };
        var predicate = Build(s => s.BooleanValue, false, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_Boolean_Descending_OrdersTrueBeforeFalse()
    {
        var sample = new TestClass { BooleanValue = false };
        var predicate = Build(s => s.BooleanValue, true, OrderingDirection.Descending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_Enum_Ascending_OrdersByUnderlyingValue()
    {
        var sample = new TestClass { EnumValue = TestEnum.C };
        var predicate = Build(s => s.EnumValue, TestEnum.B, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_Enum_Descending_OrdersByUnderlyingValue()
    {
        var sample = new TestClass { EnumValue = TestEnum.B };
        var predicate = Build(s => s.EnumValue, TestEnum.C, OrderingDirection.Descending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableEnum_WithValue_OrdersByUnderlyingValue()
    {
        var sample = new TestClass { NullableEnumValue = TestEnum.C };
        var predicate = Build(s => s.NullableEnumValue, TestEnum.B, OrderingDirection.Ascending);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_NullableEnum_NullProperty_Ascending_SortsLast()
    {
        var sample = new TestClass { NullableEnumValue = null };
        var predicate = Build(s => s.NullableEnumValue, TestEnum.A, OrderingDirection.Ascending);
        Assert.False(predicate(sample));
    }

    [Fact]
    public void Comparison_DateTime_Ascending_OrdersChronologically()
    {
        var sample = new TestClass { DateTimeValue = new DateTime(2024, 01, 02) };
        var predicate = Build(
            s => s.DateTimeValue,
            new DateTime(2024, 01, 01),
            OrderingDirection.Ascending);

        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_DateOnly_Descending_OrdersChronologically()
    {
        var sample = new TestClass { DateOnlyValue = new DateOnly(2024, 01, 01) };
        var predicate = Build(
            s => s.DateOnlyValue,
            new DateOnly(2024, 01, 02),
            OrderingDirection.Descending);

        Assert.True(predicate(sample));
    }

    [Fact]
    public void Comparison_TimeOnly_Ascending_OrdersChronologically()
    {
        var sample = new TestClass { TimeOnlyValue = new TimeOnly(12, 00) };
        var predicate = Build(
            s => s.TimeOnlyValue,
            new TimeOnly(09, 00),
            OrderingDirection.Ascending);

        Assert.True(predicate(sample));
    }

    [Fact]
    public void Equality_Int32_MatchesEqualValue()
    {
        var sample = new TestClass { Int32Value = 5 };
        var predicate = BuildEquality(s => s.Int32Value, 5);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Equality_NullableInt32_MatchesEqualValue()
    {
        var sample = new TestClass { NullableInt32Value = 5 };
        var predicate = BuildEquality(s => s.NullableInt32Value, 5);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Equality_NullableInt32_Null_MatchesNull()
    {
        var sample = new TestClass { NullableInt32Value = null };
        var predicate = BuildEquality(s => s.NullableInt32Value, null);
        Assert.True(predicate(sample));
    }

    [Fact]
    public void Equality_Enum_MatchesEqualValue()
    {
        var sample = new TestClass { EnumValue = TestEnum.C };
        var predicate = BuildEquality(s => s.EnumValue, TestEnum.C);
        Assert.True(predicate(sample));
    }

    private static Func<TestClass, bool> Build<T>(
        Expression<Func<TestClass, T>> selector,
        object? value,
        OrderingDirection direction)
    {
        var expr = ComparisonBuilder.BuildComparison(
            selector.Body,
            value,
            direction);

        return Expression
            .Lambda<Func<TestClass, bool>>(expr, selector.Parameters)
            .Compile();
    }

    private static Func<TestClass, bool> BuildEquality<T>(
        Expression<Func<TestClass, T>> selector,
        object? value)
    {
        var expr = ComparisonBuilder.BuildEquality(
            selector.Body,
            value);

        return Expression
            .Lambda<Func<TestClass, bool>>(expr, selector.Parameters)
            .Compile();
    }
}
