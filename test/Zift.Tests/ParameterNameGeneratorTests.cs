namespace Zift.Tests;

using SharedFixture.Models;

public class ParameterNameGeneratorTests
{
    [Theory]
    [InlineData(typeof(Category), "c")]
    [InlineData(typeof(Product), "p")]
    [InlineData(typeof(Review), "r")]
    [InlineData(typeof(Review[]), "r")]
    [InlineData(typeof(IList<Product>), "i")]
    [InlineData(typeof(_123Type), "t")]
    public void FromType_TypeNameStartsWithAsciiLetter_ReturnsLowercaseLetter(Type type, string expectedName)
    {
        var result = ParameterNameGenerator.FromType(type);

        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(typeof(_123))]
    [InlineData(typeof(Çĺâşş))]
    public void FromType_TypeNameWithoutAsciiLetters_ReturnsDefaultName(Type type)
    {
        var result = ParameterNameGenerator.FromType(type);

        Assert.Equal("x", result);
    }

    [Theory]
    [InlineData(typeof(Category), "c")]
    [InlineData(typeof(Product), "p")]
    [InlineData(typeof(Review), "r")]
    [InlineData(typeof(Review[]), "r")]
    [InlineData(typeof(IList<Product>), "i")]
    [InlineData(typeof(_123Type), "t")]
    public void FromTypeGeneric_TypeNameStartsWithAsciiLetter_ReturnsLowercaseLetter(Type type, string expectedName)
    {
        var fromTypeGenericMethod = typeof(ParameterNameGenerator)
            .GetMethod(nameof(ParameterNameGenerator.FromType),
                genericParameterCount: 1,
                Type.EmptyTypes)!
            .MakeGenericMethod(type);

        var result = (string?)fromTypeGenericMethod.Invoke(null, null);

        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(typeof(_123))]
    [InlineData(typeof(Çĺâşş))]
    public void FromTypeGeneric_TypeNameWithoutAsciiLetters_ReturnsDefaultName(Type type)
    {
        var fromTypeGenericMethod = typeof(ParameterNameGenerator)
            .GetMethod(nameof(ParameterNameGenerator.FromType),
                genericParameterCount: 1,
                Type.EmptyTypes)!
            .MakeGenericMethod(type);

        var result = (string?)fromTypeGenericMethod.Invoke(null, null);

        Assert.Equal("x", result);
    }

    #region Fixture
    private class _123Type;
    private class _123;
    private class Çĺâşş;
    #endregion
}
