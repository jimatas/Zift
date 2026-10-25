namespace Zift.Querying.E2E;

using Fixture;

public sealed class FilterErrorContractTests
{
    private sealed class PayloadHolder
    {
        public object? Payload { get; set; }
    }

    private class ValueHolder
    {
        public int Value { get; set; }
    }

    private sealed class HidingValueHolder : ValueHolder
    {
        public new string Value { get; set; } = string.Empty;
    }

    private sealed class WriteOnlyHolder
    {
        public int WriteOnly
        {
            set { }
        }

        public WriteOnlyHolder? Child { get; set; }

        public List<WriteOnlyHolder> Children { get; set; } = [];
    }

    private class NonPublicMembersHolder
    {
        private string PrivateName => "x";

        protected string ProtectedName => "x";

        internal string InternalName => "x";

        public string PrivateGetter { private get; set; } = "x";

        public int this[int index] => index;

        public NonPublicMembersHolder? Child { get; set; }

        public List<NonPublicMembersHolder> Children { get; set; } = [];

        public string Use() => PrivateName + ProtectedName + InternalName;
    }

    private sealed class ArrayHolder
    {
        public int Id { get; set; }

        public int[,] Grid { get; set; } = new int[0, 0];

        public int[][] Jagged { get; set; } = [];
    }

    private static IQueryable<TestClass> Source => new[] { new TestClass() }.AsQueryable();

    public static IEnumerable<object[]> InvalidFilters =>
    [
        ["Int32Value =="],
        ["Int32Value @ 1"],
        ["Int32Value == 'abc"],
        ["(Int32Value == 1"],
        ["Int32Value:total > 1"],
        ["SubEntities:all()"],

        ["Missing == 1"],
        ["NestedEntity.Missing == 1"],
        ["SubEntities:any(Missing == 1)"],

        ["StringValue == 5"],
        ["Int32Value == 'abc'"],
        ["Int32Value == true"],
        ["Int32Value %= 'a'"],
        ["Int32Value == 1.5"],
        ["Int32Value == 3000000000"],
        ["Int32Value == null"],
        ["Int32Value > null"],
        ["Int32Value in [1, null]"],
        ["EnumValue == 'Nope'"],
        ["EnumValue == 1"],
        ["GuidValue == 'nope'"],
        ["DateTimeValue == 'nope'"],
        ["DateTimeOffsetValue == 'nope'"],
        ["DateOnlyValue == 'nope'"],
        ["TimeOnlyValue == 'nope'"],

        ["BooleanValue > true"],
        ["EnumValue > 'A'"],
        ["Int32Value == [1, 2]"],
        ["Int32Value in 5"],

        ["Int32Value:any()"],
        ["StringValue:any()"],
        ["StringValue:all(Int32Value == 1)"],
        ["Int32Value:count > 1"],
        ["StringValue:count > 1"]
    ];

    [Theory]
    [MemberData(nameof(InvalidFilters))]
    public void Where_InvalidFilter_ThrowsInvalidFilterException(string filter)
    {
        Assert.ThrowsAny<InvalidFilterException>(() => Source.Where(filter));
    }

    [Fact]
    public void Where_UnknownProperty_NamesTheTypeAndTheProperty()
    {
        var ex = Assert.Throws<InvalidFilterException>(() => Source.Where("NestedEntity.Missing == 1"));

        Assert.Contains("TestClass", ex.Message);
        Assert.Contains("Missing", ex.Message);
        Assert.Null(ex.InnerException);
        Assert.Null(ex.Position);
    }

    [Theory]
    [InlineData("GuidValue == 'nope'", "Guid")]
    [InlineData("DateTimeValue == 'nope'", "DateTime")]
    [InlineData("DateTimeOffsetValue == 'nope'", "DateTimeOffset")]
    [InlineData("DateOnlyValue == 'nope'", "DateOnly")]
    [InlineData("TimeOnlyValue == 'nope'", "TimeOnly")]
    public void Where_InvalidDateTimeOrGuidText_KeepsTheParserErrorAsInnerException(string filter, string typeName)
    {
        var ex = Assert.Throws<InvalidFilterException>(() => Source.Where(filter));

        Assert.Contains("'nope'", ex.Message);
        Assert.Contains(typeName, ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Theory]
    [InlineData("Payload < 'abc'", "LessThan")]
    [InlineData("Payload <= 'abc'", "LessThanOrEqual")]
    [InlineData("Payload > 'abc'", "GreaterThan")]
    [InlineData("Payload >= 'abc'", "GreaterThanOrEqual")]
    public void Where_OrderedComparisonOnPropertyWithoutOrdering_ThrowsInvalidFilterExceptionWithTheOriginalError(
        string filter,
        string operatorName)
    {
        var ex = Assert.Throws<InvalidFilterException>(() =>
            Array.Empty<PayloadHolder>().AsQueryable().Where(filter));

        Assert.Contains($"Operator '{operatorName}'", ex.Message);
        Assert.Contains("'Object'", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Theory]
    [InlineData("Value == 1")]
    [InlineData("Value == 'x'")]
    public void Where_PropertyHiddenByAnotherType_ThrowsInvalidFilterExceptionWithTheOriginalError(string filter)
    {
        var ex = Assert.Throws<InvalidFilterException>(() =>
            Array.Empty<HidingValueHolder>().AsQueryable().Where(filter));

        Assert.Contains("'Value'", ex.Message);
        Assert.Contains("ambiguous", ex.Message);
        Assert.IsType<System.Reflection.AmbiguousMatchException>(ex.InnerException);
    }

    [Theory]
    [InlineData("WriteOnly == 1")]
    [InlineData("WriteOnly in [1]")]
    [InlineData("WriteOnly != null")]
    [InlineData("Child.WriteOnly == 1")]
    [InlineData("Children:any(WriteOnly == 1)")]
    public void Where_WriteOnlyProperty_ThrowsInvalidFilterException(string filter)
    {
        var ex = Assert.Throws<InvalidFilterException>(() =>
            Array.Empty<WriteOnlyHolder>().AsQueryable().Where(filter));

        Assert.Contains("'WriteOnly'", ex.Message);
        Assert.Contains("can't be read", ex.Message);
    }

    [Theory]
    [InlineData("PrivateName == 'x'", "PrivateName")]
    [InlineData("ProtectedName == 'x'", "ProtectedName")]
    [InlineData("InternalName == 'x'", "InternalName")]
    [InlineData("Child.PrivateName == 'x'", "PrivateName")]
    [InlineData("Child.Child.InternalName == 'x'", "InternalName")]
    [InlineData("Children:any(ProtectedName == 'x')", "ProtectedName")]
    [InlineData("Children:all(InternalName == 'x')", "InternalName")]
    public void Where_NonPublicProperty_ThrowsInvalidFilterException(string filter, string property)
    {
        var ex = Assert.Throws<InvalidFilterException>(() =>
            Array.Empty<NonPublicMembersHolder>().AsQueryable().Where(filter));

        Assert.Equal($"'NonPublicMembersHolder' has no public property '{property}'.", ex.Message);
    }

    [Theory]
    [InlineData("PrivateGetter == 'x'")]
    [InlineData("Child.PrivateGetter == 'x'")]
    [InlineData("Children:any(PrivateGetter == 'x')")]
    public void Where_PropertyWithNonPublicGetter_ThrowsInvalidFilterException(string filter)
    {
        var ex = Assert.Throws<InvalidFilterException>(() =>
            Array.Empty<NonPublicMembersHolder>().AsQueryable().Where(filter));

        Assert.Equal("Property 'PrivateGetter' on type 'NonPublicMembersHolder' can't be read.", ex.Message);
    }

    [Fact]
    public void Where_IndexedProperty_ThrowsInvalidFilterException()
    {
        var ex = Assert.Throws<InvalidFilterException>(() =>
            Array.Empty<NonPublicMembersHolder>().AsQueryable().Where("Item == 1"));

        Assert.Equal("Indexed property 'Item' on type 'NonPublicMembersHolder' can't be used.", ex.Message);
    }

    [Theory]
    [InlineData("Grid:count > 0")]
    [InlineData("Grid:any()")]
    [InlineData("Grid:any(Id == 1)")]
    [InlineData("Grid:all(Id == 1)")]
    public void Where_MultidimensionalArray_ThrowsInvalidFilterException(string filter)
    {
        Assert.Throws<InvalidFilterException>(() =>
            Array.Empty<ArrayHolder>().AsQueryable().Where(filter));
    }

    [Theory]
    [InlineData("Jagged:count > 0")]
    [InlineData("Jagged:any()")]
    public void Where_JaggedArray_IsStillSupported(string filter)
    {
        var query = Array.Empty<ArrayHolder>().AsQueryable().Where(filter);

        Assert.NotNull(query);
    }

    [Fact]
    public void Where_LexicalError_ExposesThePositionOfTheCharacter()
    {
        var ex = Assert.Throws<Parsing.LexicalErrorException>(() => Source.Where("Int32Value @ 1"));

        Assert.Equal(11, ex.Position);
    }

    [Fact]
    public void Where_SyntaxError_ExposesThePositionOfTheToken()
    {
        var ex = Assert.Throws<Parsing.SyntaxErrorException>(() => Source.Where("Int32Value == == 1"));

        Assert.Equal(14, ex.Position);
    }

    [Fact]
    public void Where_NullFilter_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Source.Where(null!));

        Assert.Equal("whereExpression", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Where_BlankFilter_ThrowsArgumentExceptionAndNotInvalidFilterException(string filter)
    {
        var ex = Assert.Throws<ArgumentException>(() => Source.Where(filter));

        Assert.Equal("whereExpression", ex.ParamName);
    }
}
