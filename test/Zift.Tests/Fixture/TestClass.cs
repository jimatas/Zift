namespace Zift.Fixture;

public class TestClass
{
    public int Int32Value { get; set; }
    public int? NullableInt32Value { get; set; }

    public double DoubleValue { get; set; }
    public double? NullableDoubleValue { get; set; }

    public decimal DecimalValue { get; set; }

    public bool BooleanValue { get; set; }
    public bool? NullableBooleanValue { get; set; }

    public TestEnum EnumValue { get; set; }
    public TestEnum? NullableEnumValue { get; set; }

    public Guid GuidValue { get; set; }
    public Guid? NullableGuidValue { get; set; }

    public string? StringValue { get; set; }

    public DateTime DateTimeValue { get; set; }
    public DateTimeOffset DateTimeOffsetValue { get; set; }
    public DateOnly DateOnlyValue { get; set; }
    public TimeOnly TimeOnlyValue { get; set; }

    public TestClass? NestedEntity { get; set; }
    public IList<TestClass> SubEntities { get; init; } = [];
    public IEnumerable NonGenericSubEntities { get; set; } = Array.Empty<object>();

}
