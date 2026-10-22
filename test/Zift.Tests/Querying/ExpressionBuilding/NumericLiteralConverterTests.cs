namespace Zift.Querying.ExpressionBuilding;

public sealed class NumericLiteralConverterTests
{
    public sealed class Integral
    {
        [Theory]
        [InlineData("0", typeof(int), 0)]
        [InlineData("-0", typeof(int), 0)]
        [InlineData("+7", typeof(int), 7)]
        [InlineData("1.0", typeof(int), 1)]
        [InlineData("1e3", typeof(int), 1000)]
        [InlineData("-1.5e1", typeof(int), -15)]
        [InlineData("2147483647", typeof(int), int.MaxValue)]
        [InlineData("-2147483648", typeof(int), int.MinValue)]
        public void Convert_IntegralValue_ToInt32_ReturnsValue(string text, Type type, int expected)
        {
            Assert.Equal(expected, NumericLiteralConverter.Convert(text, type));
        }

        [Fact]
        public void Convert_ToInt64_AcceptsValuesBeyondInt32()
        {
            Assert.Equal(3_000_000_000L, NumericLiteralConverter.Convert("3000000000", typeof(long)));
            Assert.Equal(long.MaxValue, NumericLiteralConverter.Convert("9223372036854775807", typeof(long)));
            Assert.Equal(long.MinValue, NumericLiteralConverter.Convert("-9223372036854775808", typeof(long)));
        }

        [Fact]
        public void Convert_ToUInt64_AcceptsFullRange()
        {
            Assert.Equal(ulong.MaxValue, NumericLiteralConverter.Convert("18446744073709551615", typeof(ulong)));
            Assert.Equal(0UL, NumericLiteralConverter.Convert("0", typeof(ulong)));
        }

        [Theory]
        [InlineData("255", typeof(byte))]
        [InlineData("127", typeof(sbyte))]
        [InlineData("-128", typeof(sbyte))]
        [InlineData("65535", typeof(ushort))]
        [InlineData("32767", typeof(short))]
        [InlineData("-32768", typeof(short))]
        [InlineData("4294967295", typeof(uint))]
        public void Convert_BoundaryValue_ToSmallIntegralTypes_Succeeds(string text, Type type)
        {
            var result = NumericLiteralConverter.Convert(text, type);

            Assert.IsType(type, result);
            Assert.Equal(text, System.Convert.ToString(result, System.Globalization.CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData("2147483648", typeof(int))]
        [InlineData("-2147483649", typeof(int))]
        [InlineData("9223372036854775808", typeof(long))]
        [InlineData("-9223372036854775809", typeof(long))]
        [InlineData("18446744073709551616", typeof(ulong))]
        [InlineData("-1", typeof(ulong))]
        [InlineData("-1", typeof(uint))]
        [InlineData("-1", typeof(byte))]
        [InlineData("256", typeof(byte))]
        [InlineData("128", typeof(sbyte))]
        [InlineData("-129", typeof(sbyte))]
        [InlineData("65536", typeof(ushort))]
        [InlineData("32768", typeof(short))]
        [InlineData("4294967296", typeof(uint))]
        [InlineData("1e100", typeof(int))]
        [InlineData("-1e100", typeof(long))]
        [InlineData("1e400", typeof(long))]
        public void Convert_OutOfRangeValue_ThrowsInvalidOperationException(string text, Type type)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                NumericLiteralConverter.Convert(text, type));

            Assert.Contains("out of range", ex.Message);
            Assert.Contains(text, ex.Message);
            Assert.Contains(type.Name, ex.Message);
        }

        [Theory]
        [InlineData("1.5")]
        [InlineData("-1.5")]
        [InlineData("0.1")]
        [InlineData("1e-1")]
        [InlineData("2147483647.5")]
        public void Convert_FractionalValue_ToIntegralType_ThrowsInvalidOperationException(string text)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                NumericLiteralConverter.Convert(text, typeof(int)));

            Assert.Contains("not an integer", ex.Message);
        }
    }

    public sealed class Floating
    {
        [Fact]
        public void Convert_ToDouble_ReturnsNearestValue()
        {
            Assert.Equal(0.1, NumericLiteralConverter.Convert("0.1", typeof(double)));
            Assert.Equal(1e308, NumericLiteralConverter.Convert("1e308", typeof(double)));
            Assert.Equal(-1.5e-3, NumericLiteralConverter.Convert("-1.5e-3", typeof(double)));
            Assert.Equal(5.0, NumericLiteralConverter.Convert("5", typeof(double)));
        }

        [Fact]
        public void Convert_ToFloat_ReturnsNearestValue()
        {
            Assert.Equal(0.1f, NumericLiteralConverter.Convert("0.1", typeof(float)));
            Assert.Equal(3.4e38f, NumericLiteralConverter.Convert("3.4e38", typeof(float)));
        }

        [Theory]
        [InlineData("1e999", typeof(double))]
        [InlineData("-1e999", typeof(double))]
        [InlineData("1e39", typeof(float))]
        [InlineData("-1e39", typeof(float))]
        public void Convert_OverflowingFloatingPointValue_ThrowsInvalidOperationException(string text, Type type)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                NumericLiteralConverter.Convert(text, type));

            Assert.Contains("out of range", ex.Message);
        }

        [Fact]
        public void Convert_UnderflowingDoubleValue_ReturnsZero()
        {
            Assert.Equal(0.0, NumericLiteralConverter.Convert("1e-999", typeof(double)));
        }
    }

    public sealed class Decimals
    {
        [Fact]
        public void Convert_ToDecimal_PreservesAllDigits()
        {
            const string text = "0.1234567890123456789012345678";

            Assert.Equal(0.1234567890123456789012345678m, NumericLiteralConverter.Convert(text, typeof(decimal)));
        }

        [Fact]
        public void Convert_ToDecimal_AcceptsExponentAndSign()
        {
            Assert.Equal(1500m, NumericLiteralConverter.Convert("1.5e3", typeof(decimal)));
            Assert.Equal(-0.002m, NumericLiteralConverter.Convert("-2e-3", typeof(decimal)));
        }

        [Fact]
        public void Convert_ToDecimal_AcceptsMaxValue()
        {
            Assert.Equal(decimal.MaxValue, NumericLiteralConverter.Convert("79228162514264337593543950335", typeof(decimal)));
            Assert.Equal(decimal.MinValue, NumericLiteralConverter.Convert("-79228162514264337593543950335", typeof(decimal)));
        }

        [Theory]
        [InlineData("79228162514264337593543950336")]
        [InlineData("-79228162514264337593543950336")]
        [InlineData("1e29")]
        [InlineData("1e999")]
        public void Convert_ToDecimal_OutOfRange_ThrowsInvalidOperationException(string text)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                NumericLiteralConverter.Convert(text, typeof(decimal)));

            Assert.Contains("out of range", ex.Message);
        }
    }

    public sealed class Targets
    {
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(TestEnumTarget))]
        public void Convert_ToNonNumericType_ThrowsInvalidOperationException(Type type)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                NumericLiteralConverter.Convert("1", type));

            Assert.Contains(type.Name, ex.Message);
        }

        [Fact]
        public void Convert_UnderNonInvariantCulture_StillParsesInvariantly()
        {
            var original = System.Globalization.CultureInfo.CurrentCulture;

            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

                Assert.Equal(1.5, NumericLiteralConverter.Convert("1.5", typeof(double)));
                Assert.Equal(1.5m, NumericLiteralConverter.Convert("1.5", typeof(decimal)));
                Assert.Equal(1500, NumericLiteralConverter.Convert("1.5e3", typeof(int)));
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = original;
            }
        }

        private enum TestEnumTarget
        {
            A
        }
    }

    public sealed class EndToEnd
    {
        private static IQueryable<Item> Source => new[]
        {
            new Item { Small = 1, Big = 3_000_000_000, Money = 0.1234567890123456789012345678m, Ratio = 0.5, Unsigned = ulong.MaxValue, Id = 1 },
            new Item { Small = 2, Big = 5, Money = 1m, Ratio = 1.5, Unsigned = 0, Id = 2 }
        }.AsQueryable();

        private static int[] Ids(string filter) =>
            Source.Where(filter).Select(i => i.Id).OrderBy(i => i).ToArray();

        [Fact]
        public void Where_LongProperty_AcceptsValueBeyondInt32()
        {
            Assert.Equal([1], Ids("Big == 3000000000"));
            Assert.Equal([1], Ids("Big > 2999999999"));
            Assert.Equal([1], Ids("Big in [3000000000, 4000000000]"));
        }

        [Fact]
        public void Where_Int32Property_WithValueBeyondInt32_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => Ids("Small == 3000000000"));

            Assert.Contains("out of range", ex.Message);
        }

        [Fact]
        public void Where_Int32Property_WithFractionalValue_ThrowsInsteadOfRounding()
        {
            Assert.Throws<InvalidOperationException>(() => Ids("Small == 1.5"));
            Assert.Throws<InvalidOperationException>(() => Ids("Small > 1.5"));
            Assert.Throws<InvalidOperationException>(() => Ids("Small in [1, 1.5]"));
        }

        [Fact]
        public void Where_Int32Property_WithIntegralFloatingPointText_Matches()
        {
            Assert.Equal([1], Ids("Small == 1.0"));
            Assert.Equal([2], Ids("Small == 2e0"));
        }

        [Fact]
        public void Where_DecimalProperty_KeepsFullPrecision()
        {
            Assert.Equal([1], Ids("Money == 0.1234567890123456789012345678"));
            Assert.Empty(Ids("Money == 0.1234567890123456789012345679"));
        }

        [Fact]
        public void Where_UnsignedLongProperty_AcceptsMaxValue()
        {
            Assert.Equal([1], Ids("Unsigned == 18446744073709551615"));
            Assert.Throws<InvalidOperationException>(() => Ids("Unsigned == -1"));
        }

        [Fact]
        public void Where_DoubleProperty_WithOverflowingLiteral_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Ids("Ratio < 1e999"));
        }

        [Fact]
        public void Where_ListWithOneOutOfRangeItem_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Ids("Small in [1, 2, 3000000000]"));
        }

        private sealed class Item
        {
            public int Id { get; set; }
            public int Small { get; set; }
            public long Big { get; set; }
            public decimal Money { get; set; }
            public double Ratio { get; set; }
            public ulong Unsigned { get; set; }
        }
    }
}
