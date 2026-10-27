using Zift.Fixture;

namespace Zift.Filtering.ExpressionBuilding;

public sealed class StringLiteralConverterTests
{
    [Flags]
    private enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4
    }

    public sealed class Passthrough
    {
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        public void Convert_ToStringOrObject_ReturnsValueUnchanged(Type type)
        {
            Assert.Equal("hello", StringLiteralConverter.Convert("hello", type));
            Assert.Equal(string.Empty, StringLiteralConverter.Convert(string.Empty, type));
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(decimal))]
        public void Convert_ToUnsupportedType_ThrowsInvalidFilterException(Type type)
        {
            var ex = Assert.Throws<InvalidFilterException>(() =>
                StringLiteralConverter.Convert("1", type));

            Assert.Contains(type.Name, ex.Message);
        }
    }

    public sealed class Parsers
    {
        [Fact]
        public void Convert_ToGuid_ParsesAllCommonFormats()
        {
            var id = Guid.NewGuid();

            Assert.Equal(id, StringLiteralConverter.Convert(id.ToString("D"), typeof(Guid)));
            Assert.Equal(id, StringLiteralConverter.Convert(id.ToString("N"), typeof(Guid)));
            Assert.Equal(id, StringLiteralConverter.Convert(id.ToString("B"), typeof(Guid)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-guid")]
        [InlineData("1234")]
        public void Convert_ToGuid_WithInvalidText_ThrowsInvalidFilterException(string text)
        {
            Assert.Throws<InvalidFilterException>(() =>
                StringLiteralConverter.Convert(text, typeof(Guid)));
        }

        [Fact]
        public void Convert_ToDateTime_PreservesKind()
        {
            var utc = (DateTime)StringLiteralConverter.Convert("2024-03-05T10:20:30Z", typeof(DateTime));
            var unspecified = (DateTime)StringLiteralConverter.Convert("2024-03-05T10:20:30", typeof(DateTime));

            Assert.Equal(DateTimeKind.Utc, utc.Kind);
            Assert.Equal(new DateTime(2024, 3, 5, 10, 20, 30, DateTimeKind.Utc), utc);
            Assert.Equal(DateTimeKind.Unspecified, unspecified.Kind);
        }

        [Fact]
        public void Convert_ToDateTimeOffset_PreservesOffset()
        {
            var value = (DateTimeOffset)StringLiteralConverter.Convert("2024-03-05T10:20:30+02:00", typeof(DateTimeOffset));

            Assert.Equal(TimeSpan.FromHours(2), value.Offset);
            Assert.Equal(new DateTimeOffset(2024, 3, 5, 10, 20, 30, TimeSpan.FromHours(2)), value);
        }

        [Fact]
        public void Convert_ToDateOnlyAndTimeOnly_ParsesInvariantFormats()
        {
            Assert.Equal(new DateOnly(2024, 3, 5), StringLiteralConverter.Convert("2024-03-05", typeof(DateOnly)));
            Assert.Equal(new TimeOnly(10, 20, 30), StringLiteralConverter.Convert("10:20:30", typeof(TimeOnly)));
        }

        [Theory]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(DateOnly))]
        [InlineData(typeof(TimeOnly))]
        public void Convert_ToDateOrTimeType_WithInvalidText_ThrowsInvalidFilterException(Type type)
        {
            Assert.Throws<InvalidFilterException>(() =>
                StringLiteralConverter.Convert("not a date", type));
        }

        [Fact]
        public void Convert_ToDateOnly_UnderNonInvariantCulture_StillParsesInvariantly()
        {
            var original = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                Assert.Equal(new DateOnly(2024, 3, 5), StringLiteralConverter.Convert("2024-03-05", typeof(DateOnly)));
                Assert.Equal(new DateTime(2024, 3, 5), StringLiteralConverter.Convert("2024-03-05", typeof(DateTime)));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }
    }

    public sealed class Enums
    {
        [Theory]
        [InlineData("A", TestEnum.A)]
        [InlineData("B", TestEnum.B)]
        [InlineData("D", TestEnum.D)]
        public void Convert_ToEnum_WithDefinedName_ReturnsMember(string text, TestEnum expected)
        {
            Assert.Equal(expected, StringLiteralConverter.Convert(text, typeof(TestEnum)));
        }

        [Fact]
        public void Convert_ToEnum_IgnoresSurroundingWhitespace()
        {
            Assert.Equal(TestEnum.B, StringLiteralConverter.Convert(" B ", typeof(TestEnum)));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("0")]
        [InlineData("99")]
        [InlineData("-1")]
        [InlineData("+1")]
        [InlineData("1.0")]
        [InlineData("0x1")]
        public void Convert_ToEnum_WithNumericText_ThrowsInvalidFilterException(string text)
        {
            var ex = Assert.Throws<InvalidFilterException>(() =>
                StringLiteralConverter.Convert(text, typeof(TestEnum)));

            Assert.Contains("not a defined value", ex.Message);
            Assert.Contains(nameof(TestEnum), ex.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("E")]
        [InlineData("b")]
        [InlineData("TestEnum.B")]
        [InlineData("A,B")]
        [InlineData("A, B")]
        [InlineData("A,")]
        [InlineData(",A")]
        public void Convert_ToEnum_WithUndefinedWrongCaseOrListText_ThrowsInvalidFilterException(string text)
        {
            Assert.Throws<InvalidFilterException>(() =>
                StringLiteralConverter.Convert(text, typeof(TestEnum)));
        }

        [Fact]
        public void Convert_ToFlagsEnum_WithSingleName_ReturnsMember()
        {
            Assert.Equal(Permissions.Write, StringLiteralConverter.Convert("Write", typeof(Permissions)));
            Assert.Equal(Permissions.None, StringLiteralConverter.Convert("None", typeof(Permissions)));
        }

        [Theory]
        [InlineData("Read,Write")]
        [InlineData("Read, Write")]
        [InlineData(" Read , Execute ")]
        public void Convert_ToFlagsEnum_WithDefinedNameList_ReturnsCombination(string text)
        {
            var result = (Permissions)StringLiteralConverter.Convert(text, typeof(Permissions));

            Assert.True(result.HasFlag(Permissions.Read));
            Assert.False(result.HasFlag(Permissions.Write) && result.HasFlag(Permissions.Execute));
        }

        [Fact]
        public void Convert_ToFlagsEnum_WithAllNames_ReturnsUnion()
        {
            Assert.Equal(
                Permissions.Read | Permissions.Write | Permissions.Execute,
                StringLiteralConverter.Convert("Read, Write, Execute", typeof(Permissions)));
        }

        [Theory]
        [InlineData("3")]
        [InlineData("16")]
        [InlineData("Read,Bogus")]
        [InlineData("Read,8")]
        [InlineData("Read,")]
        [InlineData("read")]
        [InlineData("")]
        public void Convert_ToFlagsEnum_WithNumericOrUndefinedText_ThrowsInvalidFilterException(string text)
        {
            Assert.Throws<InvalidFilterException>(() =>
                StringLiteralConverter.Convert(text, typeof(Permissions)));
        }
    }

    public sealed class EndToEnd
    {
        private static IQueryable<TestClass> Source => new[]
        {
            new TestClass { Int32Value = 1, EnumValue = TestEnum.A, NullableEnumValue = TestEnum.B },
            new TestClass { Int32Value = 2, EnumValue = TestEnum.B, NullableEnumValue = null },
            new TestClass { Int32Value = 3, EnumValue = (TestEnum)99, NullableEnumValue = TestEnum.D }
        }.AsQueryable();

        private static int[] Ids(string filter) =>
            Source.Where(filter).Select(i => i.Int32Value).OrderBy(i => i).ToArray();

        [Fact]
        public void Where_EnumPropertyAndDefinedName_Matches()
        {
            Assert.Equal([2], Ids("EnumValue == 'B'"));
            Assert.Equal([1, 3], Ids("EnumValue != 'B'"));
            Assert.Equal([1, 2], Ids("EnumValue in ['A', 'B']"));
        }

        [Fact]
        public void Where_NullableEnumProperty_MatchesDefinedNameAndNull()
        {
            Assert.Equal([1], Ids("NullableEnumValue == 'B'"));
            Assert.Equal([2], Ids("NullableEnumValue == null"));
        }

        [Theory]
        [InlineData("EnumValue == '1'")]
        [InlineData("EnumValue == '99'")]
        [InlineData("EnumValue in ['A', '99']")]
        [InlineData("NullableEnumValue == '3'")]
        [InlineData("EnumValue == 'A,B'")]
        public void Where_EnumPropertyWithNumericOrListText_ThrowsInvalidFilterException(string filter)
        {
            Assert.Throws<InvalidFilterException>(() => Ids(filter));
        }

        [Fact]
        public void Where_EnumPropertyWithNumberLiteral_ThrowsInvalidFilterException()
        {
            Assert.Throws<InvalidFilterException>(() => Ids("EnumValue == 1"));
        }
    }
}
