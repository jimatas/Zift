namespace Zift.Tests.Fixture;

using Filtering.Dynamic;

public class ComparisonOperatorData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "==", ComparisonOperatorType.Equal };
        yield return new object[] { "!=", ComparisonOperatorType.NotEqual };
        yield return new object[] { ">", ComparisonOperatorType.GreaterThan };
        yield return new object[] { ">=", ComparisonOperatorType.GreaterThanOrEqual };
        yield return new object[] { "<", ComparisonOperatorType.LessThan };
        yield return new object[] { "<=", ComparisonOperatorType.LessThanOrEqual };
        yield return new object[] { "%=", ComparisonOperatorType.Contains };
        yield return new object[] { "^=", ComparisonOperatorType.StartsWith };
        yield return new object[] { "$=", ComparisonOperatorType.EndsWith };
        yield return new object[] { "in", ComparisonOperatorType.In };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
