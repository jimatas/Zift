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

        void action() => type.GetPropertyIgnoreCase("PublicProperty");

        Assert.Throws<NullReferenceException>(action);
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
