namespace Zift.Tests;

public class TypeExtensionsTests
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
        // Arrange
        var type = typeof(TestClass);

        // Act
        var propertyInfo = type.GetPropertyIgnoreCase(propertyName);

        // Assert
        Assert.NotNull(propertyInfo);
        Assert.Equal(expectedPropertyName, propertyInfo.Name);
    }

    [Theory]
    [InlineData("PrivateProperty")]
    [InlineData("StaticProperty")]
    [InlineData("NonExistentProperty")]
    public void GetPropertyIgnoreCase_UnmatchedOrInvalidPropertyName_ReturnsNull(string propertyName)
    {
        // Arrange
        var type = typeof(TestClass);

        // Act
        var propertyInfo = type.GetPropertyIgnoreCase(propertyName);

        // Assert
        Assert.Null(propertyInfo);
    }

    [Fact]
    public void GetPropertyIgnoreCase_NullType_ThrowsNullReferenceException()
    {
        // Arrange
        Type type = null!;

        // Act
        void action() => type.GetPropertyIgnoreCase("PublicProperty");

        // Assert
        Assert.Throws<NullReferenceException>(action);
    }

    [Theory]
    [InlineData(typeof(TestClass), "t")]
    [InlineData(typeof(string), "s")]
    [InlineData(typeof(int), "i")]
    [InlineData(typeof(IList<TestClass>), "i")]
    public void GenerateParameterName_TypeNameStartsWithAsciiLetter_ReturnsLowercaseLetter(Type type, string expectedName)
    {
        // Arrange

        // Act
        var result = type.GenerateParameterName();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(typeof(_123))]
    [InlineData(typeof(Çĺâşş))]
    public void GenerateParameterName_TypeNameWithoutAsciiLetters_ReturnsDefaultName(Type type)
    {
        // Arrange

        // Act
        var result = type.GenerateParameterName();

        // Assert
        Assert.Equal("x", result);
    }

    #region Fixture
    public class TestClass
    {
        public string PublicProperty { get; set; } = "Public";
        private string PrivateProperty { get; set; } = "Private";
        public static string StaticProperty { get; set; } = "Static";
        public int DiffersOnlyByCase { get; set; }
        public int DiffersOnlyByCASE { get; set; }
    }
    public class _123;
    public class Çĺâşş;
    #endregion
}
