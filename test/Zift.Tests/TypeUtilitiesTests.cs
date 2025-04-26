namespace Zift.Tests;

public class TypeUtilitiesTests
{
    [Theory]
    [InlineData("PublicProperty", "publicproperty")]
    [InlineData("PublicProperty", "PUBLICPROPERTY")]
    [InlineData("PublicProperty", "PubLicPropErty")]
    [InlineData("DiffersOnlyByCase", "DiffersOnlyByCase")]
    [InlineData("DiffersOnlyByCASE", "DiffersOnlyByCASE")]
    [InlineData("DiffersOnlyByCase", "differsonlybycase")]
    public void GetPropertyIgnoreCase_PropertyNameWithDifferentCasing_ReturnsExpectedProperty(
        string expectedPropertyName,
        string propertyName)
    {
        var type = typeof(TestClass);

        var propertyInfo = type.GetPropertyIgnoreCase(propertyName);

        Assert.NotNull(propertyInfo);
        Assert.Equal(expectedPropertyName, propertyInfo.Name);
    }

    [Theory]
    [InlineData("PrivateProperty")]
    [InlineData("StaticProperty")]
    [InlineData("NonExistentProperty")]
    public void GetPropertyIgnoreCase_UnmatchedOrInvalidPropertyName_ReturnsNull(string propertyName)
    {
        var type = typeof(TestClass);

        var propertyInfo = type.GetPropertyIgnoreCase(propertyName);

        Assert.Null(propertyInfo);
    }

    [Fact]
    public void GetPropertyIgnoreCase_NullType_ThrowsNullReferenceException()
    {
        Type type = null!;

        var ex = Assert.Throws<NullReferenceException>(() => type.GetPropertyIgnoreCase("PublicProperty"));
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(double))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(DateTime))]
    public void IsNullableType_TypeIsNotNullable_ReturnsFalse(Type type)
    {
        Assert.False(type.IsNullableType());
    }

    [Theory]
    [InlineData(typeof(Guid?))]
    [InlineData(typeof(int?))]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(string))]
    [InlineData(typeof(IEnumerable<string>))]
    [InlineData(typeof(TestClass))]
    public void IsNullableType_TypeIsNullable_ReturnsTrue(Type type)
    {
        Assert.True(type.IsNullableType());
    }

    [Theory]
    [InlineData(typeof(IEnumerable))]
    [InlineData(typeof(IEnumerable<TestClass>))]
    [InlineData(typeof(ICollection<TestClass>))]
    [InlineData(typeof(TestClass[]))]
    public void IsCollectionType_TypeIsCollection_ReturnsTrue(Type type)
    {
        Assert.True(type.IsCollectionType());
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(TestClass))]
    public void IsCollectionType_TypeIsNotCollection_ReturnsFalse(Type type)
    {
        Assert.False(type.IsCollectionType());
    }

    [Theory]
    [InlineData(typeof(IEnumerable<TestClass>), typeof(TestClass))]
    [InlineData(typeof(IList<TestClass>), typeof(TestClass))]
    [InlineData(typeof(List<TestClass>), typeof(TestClass))]
    [InlineData(typeof(TestClass[]), typeof(TestClass))]
    [InlineData(typeof(IDictionary<int, TestClass>), typeof(KeyValuePair<int, TestClass>))]
    [InlineData(typeof(Dictionary<int, TestClass>), typeof(KeyValuePair<int, TestClass>))]
    public void GetCollectionElementType_TypeIsCollection_ReturnsElementType(Type collectionType, Type elementType)
    {
        var result = collectionType.GetCollectionElementType();

        Assert.NotNull(result);
        Assert.Same(elementType, result);
    }

    [Theory]
    [InlineData(typeof(IEnumerable))]
    [InlineData(typeof(IList))]
    [InlineData(typeof(ArrayList))]
    [InlineData(typeof(IDictionary))]
    public void GetCollectionElementType_TypeIsNonGenericCollection_ReturnsNull(Type collectionType)
    {
        Assert.Null(collectionType.GetCollectionElementType());
    }

    [Fact]
    public void GetCollectionElementType_TypeIsNotCollection_ReturnsNull()
    {
        var type = typeof(TestClass);

        Assert.Null(type.GetCollectionElementType());
    }

    #region Fixture
    private class TestClass
    {
        public string PublicProperty { get; set; } = "Public";
        private string PrivateProperty { get; set; } = "Private";
        public static string StaticProperty { get; set; } = "Static";
        public int DiffersOnlyByCase { get; set; }
        public int DiffersOnlyByCASE { get; set; }
    }
    #endregion
}
