namespace Zift.Sorting;

internal static class PropertyAccessExtensions
{
    public static LambdaExpression ToPropertySelector<T>(this string property)
    {
        var type = typeof(T);
        var parameter = Expression.Parameter(type, type.GenerateParameterName());
        Expression expression = parameter;

        foreach (var nestedProperty in property.Split('.'))
        {
            var propertyInfo = type.GetPropertyIgnoreCase(nestedProperty)
                ?? throw new ArgumentException(
                    $"No accessible property '{nestedProperty}' defined on type '{type.Name}'.",
                    nameof(property));

            expression = Expression.Property(expression, propertyInfo);
            type = propertyInfo.PropertyType;
        }

        return Expression.Lambda(expression, parameter);
    }

    public static string? ToPropertyPath(this LambdaExpression property)
    {
        return BuildPropertyPath(property.Body);
    }

    private static string? BuildPropertyPath(Expression? expression)
    {
        return expression switch
        {
            MemberExpression member => $"{BuildPropertyPath(member.Expression)}.{member.Member.Name}".TrimStart('.'),
            UnaryExpression { NodeType: ExpressionType.Convert } unary => BuildPropertyPath(unary.Operand),
            _ => null
        };
    }
}
