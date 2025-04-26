namespace Zift.Tests.Fixture;

using Filtering.Dynamic;
using Filtering.Dynamic.Parsing;

public static class PropertyPathFactory
{
    public static PropertyPath Create(string propertyPath)
    {
        return new ExpressionParser(new($"{propertyPath} != null"))
            .Parse()
            .Terms
            .OfType<FilterCondition>()
            .Select(c => c.Property)
            .First();
    }
}
