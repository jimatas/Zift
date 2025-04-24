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
        var path = new Stack<string>();

        while (expression is not null)
        {
            switch (expression)
            {
                case MemberExpression member:
                    path.Push(member.Member.Name);
                    expression = member.Expression;
                    break;

                case UnaryExpression { NodeType: ExpressionType.Convert } unary:
                    expression = unary.Operand;
                    break;

                default:
                    return null;
            }
        }

        return path.Count > 0
            ? string.Join('.', path)
            : null;
    }
}
