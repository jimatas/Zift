﻿namespace Zift.Tests;

using Filtering.Dynamic;
using Filtering.Dynamic.Parsing;

public class ExpressionParserTests
{
    [Fact]
    public void Parse_EmptyExpression_ThrowsSyntaxErrorException()
    {
        var parser = new ExpressionParser(new(string.Empty));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.Equal("Empty expression detected: An expression must contain at least one term.", ex.Message);
    }

    [Fact]
    public void Parse_CalledTwice_ThrowsSyntaxErrorException()
    {
        var parser = new ExpressionParser(new("Name == 'Laptop'"));
        parser.Parse();

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.Equal("Empty expression detected: An expression must contain at least one term.", ex.Message);
    }

    [Fact]
    public void Parse_ValidExpression_ReturnsFilterGroup()
    {
        var parser = new ExpressionParser(new("Price > 500 && Name ^= 'S'"));

        var result = parser.Parse();

        Assert.NotNull(result);
        Assert.Equal(2, result.Terms.Count);
        Assert.All(result.Terms, condition => Assert.IsType<FilterCondition>(condition));
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("\"\"")]
    [InlineData("''")]
    [InlineData("==")]
    [InlineData("!=")]
    public void Parse_InvalidTopLevelToken_ThrowsSyntaxErrorException(string expression)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("Unexpected token", ex.Message);
    }

    [Theory]
    [InlineData("(Price > 500 && Name == 'Smartphone'", "Mismatched parentheses")]
    [InlineData("Price > 500 && Name == 'Smartphone')", "Mismatched parentheses: Unexpected closing parenthesis")]
    public void Parse_MismatchedParentheses_ThrowsSyntaxErrorException(string expression, string expectedPartialMessage)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith(expectedPartialMessage, ex.Message);
    }

    [Fact]
    public void Parse_MissingComparisonOperator_ThrowsSyntaxErrorException()
    {
        var parser = new ExpressionParser(new("Name 'Laptop'"));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("Expected a comparison operator", ex.Message);
    }

    [Fact]
    public void Parse_UnknownComparisonOperator_ThrowsSyntaxErrorException()
    {
        var parser = new ExpressionParser(new("Name += 'Laptop'"));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("Expected a comparison operator", ex.Message);
    }

    [Fact]
    public void Parse_UnaryOperatorNotFollowedByParenthesis_ThrowsSyntaxErrorException()
    {
        var parser = new ExpressionParser(new("!Name == 'Laptop'"));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("Unary logical operator must be followed by an opening parenthesis", ex.Message);
    }

    [Fact]
    public void Parse_UnaryOperatorWithWhitespaceFollowedByParenthesis_ReturnsFilterGroup()
    {
        var parser = new ExpressionParser(new("! (Price > 1000)"));

        var result = parser.Parse();

        Assert.NotNull(result);
    }

    [Fact]
    public void Parse_SingleNestedGroup_ReturnsInnerGroup()
    {
        var parser = new ExpressionParser(new("(Name == 'Smartphone')"));

        var result = parser.Parse();

        Assert.NotNull(result);
        Assert.Single(result.Terms);
        Assert.IsType<FilterCondition>(result.Terms[0]);
    }

    [Theory]
    [InlineData("Price > 13 && ", "Expected a term after logical operator.")]
    [InlineData("&& Price > 13", "Unexpected logical operator.")]
    [InlineData("Price > 13 && || Price < 20", "Expected a term after logical operator.")]
    [InlineData("Price > ", "Unexpected token type while parsing value.")]
    [InlineData("Price", "Expected a comparison operator")]
    [InlineData("Price > 13 (Price < 20)", "Expected a logical operator between terms.")]
    [InlineData("Price > 13 && (Price < 20", "Mismatched parentheses: Missing closing parenthesis.")]
    [InlineData("Price > 13 && Price < 20)", "Mismatched parentheses: Unexpected closing parenthesis.")]
    [InlineData("!Price > 13", "Unary logical operator must be followed by an opening parenthesis.")]
    [InlineData("Price >> 13", "Unexpected token type while parsing value.")]
    [InlineData("Name == 'Smartphone' && ", "Expected a term after logical operator.")]
    [InlineData("&& Name == 'Smartphone'", "Unexpected logical operator.")]
    [InlineData("Name == 'Smartphone' && || Price > 20", "Expected a term after logical operator.")]
    [InlineData("Name", "Expected a comparison operator")]
    [InlineData("Name == 'Smartphone' (Price > 20)", "Expected a logical operator between terms.")]
    [InlineData("Name == 'Smartphone' && (Price > 20", "Mismatched parentheses: Missing closing parenthesis.")]
    [InlineData("Name == 'Smartphone' && Price > 20)", "Mismatched parentheses: Unexpected closing parenthesis.")]
    [InlineData("!Name == 'Smartphone'", "Unary logical operator must be followed by an opening parenthesis.")]
    [InlineData("Price << 20", "Unexpected token type while parsing value.")]
    [InlineData("Name == ", "Unexpected token type while parsing value.")]
    [InlineData("Name == 'Smartphone' && Content", "Expected a comparison operator, but got:")]
    [InlineData("Name == 'Smartphone' && && Price > 20", "Expected a term after logical operator.")]
    [InlineData("Name == 'Smartphone' && (Content == 'Nice' &&", "Expected a term after logical operator.")]
    [InlineData("Name == 'Smartphone' && (Content == 'Nice' && Price", "Expected a comparison operator, but got:")]
    [InlineData("()", "Empty group detected: A group must contain at least one term.")]
    [InlineData("Name == 'Smartphone' && ()", "Empty group detected: A group must contain at least one term.")]
    [InlineData("Name == 'Smartphone' && (Price > 20 && ())", "Empty group detected: A group must contain at least one term.")]
    [InlineData("Name == 'Smartphone' && (Price > 20 && (Content == 'Nice' && ()))", "Empty group detected: A group must contain at least one term.")]
    [InlineData("Name == 'Smartphone' && (Price > 20 && (Content == 'Nice' && ())", "Empty group detected: A group must contain at least one term.")]
    [InlineData("Name == 'Smartphone' && (Price > 20 && (Content == 'Nice' && (Price", "Expected a comparison operator, but got:")]
    [InlineData("Name == 'Smartphone' && (Price > 20 && (Content == 'Nice' && (Price >)))", "Unexpected token type while parsing value.")]
    [InlineData("Name == 'Smartphone' && (Price > 20 && (Content == 'Nice' && (Price > 30 &&)))", "Expected a term after logical operator.")]
    [InlineData("Name == 'Smartphone' && (Price > 20 && (Content == 'Nice' && (Price > 30 && Content)))", "Expected a comparison operator, but got:")]
    public void Parse_InvalidExpression_ThrowsSyntaxErrorException(string expression, string expectedPartialMessage)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith(expectedPartialMessage, ex.Message);
    }

    [Theory]
    [InlineData("Products.0 == 'Any value'", "Expected an identifier, but got:")]
    [InlineData("Products.[0] == 'Any value'", "Expected an identifier, but got:")]
    [InlineData("Products.Reviews.* == 'Any value'", "Expected an identifier, but got:")]
    [InlineData("Products.Reviews.% == 'Any value'", "Expected an identifier, but got:")]
    [InlineData("Products.. == 'Any value'", "Expected an identifier, but got:")]
    [InlineData("Products.: == 'Any value'", "Expected an identifier, but got:")]
    [InlineData("Products:: == 'Any value'", "Expected an identifier, but got:")]
    public void Parse_InvalidNestedProperties_ThrowsSyntaxErrorException(string expression, string expectedPartialMessage)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith(expectedPartialMessage, ex.Message);
    }

    [Theory]
    [InlineData("Name == 'Smartphone'")]
    [InlineData("Name != 'Laptop'")]
    [InlineData("Name != null")]
    [InlineData("Price > 100 && Price < 2000")]
    [InlineData("Price >= 999.99")]
    [InlineData("Name == 'Laptop' && Price > 1000")]
    [InlineData("Name == 'Refrigerator' || Price < 500")]
    [InlineData("!(Price < 600)")]
    [InlineData("Price > 100 && (Price < 2000 || Name == 'Smartphone')")]
    [InlineData("Price > 100 || (Name == 'Laptop' && Price < 2000)")]
    [InlineData("Name == 'Laptop' && Price > 1000 && Price < 2000")]
    [InlineData("Price != 500")]
    [InlineData("Name != 'Laptop' || Price <= 200")]
    [InlineData("(Name == 'Laptop' || Name == 'Smartphone') && Price > 500")]
    [InlineData("Name ^= 'S' && Price > 100")]
    [InlineData("Name $= 'top' && Price < 1500")]
    [InlineData("(Price > 500 && Price < 1500) && Name %= 'Laptop'")]
    [InlineData("Name == 'Laptop' && Price > 1000 && Price < 2000 && Name == 'Laptop'")]
    [InlineData("Price > 900 && Name == 'Smartphone' && Name ^= 'S'")]
    [InlineData("!(Name == 'Laptop' && Price < 1000)")]
    [InlineData("Price > 100 && !(Name == 'Laptop' && Price == 1299.99)")]
    [InlineData("Name == 'Laptop' && Price > 1000 || Price < 600")]
    [InlineData("Name == 'Laptop' && (Price > 1000 || Price < 500)")]
    [InlineData("Name == 'Laptop' && Price > 1000 && Name != 'Fridge'")]
    [InlineData("Name == 'Laptop' && (Name == 'Laptop' || Name == 'Smartphone')")]
    [InlineData("Name == 'Laptop' && Price > 1000 && (Name == 'Smartphone' || Price < 1500)")]
    [InlineData("(Name == 'Laptop' && Price > 1000) || (Name == 'Fridge' && Price <= 600)")]
    [InlineData("Name ^= 'Sm' && Name $= 'one'")]
    [InlineData("Name %= 'Laptop' && Price > 1000")]
    public void Parse_ValidComplexExpression_ReturnsFilterGroup(string expression)
    {
        var parser = new ExpressionParser(new(expression));

        var result = parser.Parse();

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("Products:count == 0")]
    [InlineData("Products:count > 2")]
    [InlineData("Products.Reviews:count == 5")]
    public void Parse_ValidCollectionProjection_ReturnsFilterCondition(string expression)
    {
        var parser = new ExpressionParser(new(expression));

        var result = parser.Parse();

        Assert.NotNull(result);
        Assert.Single(result.Terms);
        Assert.IsType<FilterCondition>(result.Terms[0]);
    }

    [Theory]
    [InlineData("Products:count.Reviews == 1")]
    [InlineData("Products:count.Reviews:count == 1")]
    public void Parse_InvalidTerminalProjectionPlacement_ThrowsSyntaxErrorException(string expression)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("Terminal collection projections (e.g., ':count') must appear at the end of the property path.", ex.Message);
    }

    [Theory]
    [InlineData("Products:count:count == 1")]
    [InlineData("Products:all:count == 1")]
    [InlineData("Products:all:any.Reviews.all.Rating > 5")]
    public void Parse_MultipleModifiersInSegment_ThrowsSyntaxErrorException(string expression)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("A property segment cannot have more than one modifier.", ex.Message);
    }

    [Theory]
    [InlineData("Name ==:i 'Laptop'", "==", "i")]
    [InlineData("Name == :i 'Laptop'", "==", "i")]
    [InlineData("Name == : i 'Laptop'", "==", "i")]
    [InlineData("Name ==:I 'Smartphone'", "==", "i")]
    [InlineData("Name ==:i ''", "==", "i")]
    public void Parse_StringLiteralWithOperatorModifier_ReturnsExpectedOperatorAndModifier(string expression, string expectedOperator, string expectedModifier)
    {
        var parser = new ExpressionParser(new(expression));
        var result = parser.Parse();

        var condition = Assert.IsType<FilterCondition>(Assert.Single(result.Terms));
        var op = condition.Operator;

        Assert.Equal(expectedOperator, op.Type.Symbol);
        Assert.Contains(expectedModifier, op.Modifiers);
    }

    [Theory]
    [InlineData("Products.Price >:i 42", ">")]
    [InlineData("Products.Price >=:i 42", ">=")]
    [InlineData("Products.Price <:i 42", "<")]
    [InlineData("Products.Price <=:i 42", "<=")]
    public void Parse_ModifierOnUnsupportedOperator_ThrowsSyntaxErrorException(string expression, string @operator)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith($"The '{@operator}' operator does not support the following modifier(s)", ex.Message);
    }

    [Fact]
    public void Parse_UnsupportedModifier_ThrowsSyntaxError()
    {
        var parser = new ExpressionParser(new("Name ==:xyz 'Laptop'"));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("The '==' operator does not support", ex.Message);
    }

    [Theory]
    [InlineData("Name ==:42 'Laptop'", "Expected a modifier after colon")]
    [InlineData("Name ==:'i' 'Laptop'", "Expected a modifier after colon")]
    [InlineData("Name ==:== 'Laptop'", "Expected a modifier after colon")]
    [InlineData("Name ==: 'Laptop'", "Expected a modifier after colon")]
    [InlineData("Name ==:!i 'Laptop'", "Expected a modifier after colon")]
    public void Parse_IllegalModifier_ThrowsSyntaxError(string expression, string expectedMessage)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith(expectedMessage, ex.Message);
    }

    [Theory]
    [InlineData("Name in ['Laptop', 'Smartphone']")]
    [InlineData("Price in [100, 200, 300]")]
    [InlineData("Category.Name in ['Electronics', 'Appliances']")]
    public void Parse_InOperatorWithListLiteral_ReturnsFilterCondition(string expression)
    {
        var parser = new ExpressionParser(new(expression));

        var result = parser.Parse();

        var condition = Assert.IsType<FilterCondition>(Assert.Single(result.Terms));
        Assert.Equal(ComparisonOperatorType.In, condition.Operator.Type);
        Assert.IsType<IEnumerable>(condition.Value, exactMatch: false);
    }

    [Fact]
    public void Parse_InOperatorWithScalarValue_ThrowsArgumentException()
    {
        var parser = new ExpressionParser(new("Name in 'Laptop'"));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith("Expected an opening bracket, but got", ex.Message);
    }

    [Fact]
    public void Parse_InOperatorWithEmptyList_ReturnsFilterCondition()
    {
        var parser = new ExpressionParser(new("Name in []"));

        var result = parser.Parse();

        var condition = Assert.IsType<FilterCondition>(Assert.Single(result.Terms));

        Assert.Equal(ComparisonOperatorType.In, condition.Operator.Type);
        Assert.IsType<IEnumerable>(condition.Value, exactMatch: false);
        Assert.Empty((IEnumerable)condition.Value!);
    }

    [Theory]
    [InlineData("Name in [", "Unexpected token type while parsing value.")]
    [InlineData("Name in ['Laptop'", "Expected a comma between values, but got")]
    [InlineData("Name in Laptop]", "Expected an opening bracket, but got")]
    [InlineData("Name in ['Laptop', ]", "Unexpected closing bracket")]
    [InlineData("Name in [,]", "Unexpected token type while parsing value.")]
    [InlineData("Name in [,,]", "Unexpected token type while parsing value.")]
    public void Parse_MalformedInList_ThrowsSyntaxError(string expression, string expectedMessage)
    {
        var parser = new ExpressionParser(new(expression));

        var ex = Assert.Throws<SyntaxErrorException>(parser.Parse);

        Assert.StartsWith(expectedMessage, ex.Message);
    }
}
