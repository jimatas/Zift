namespace Zift.Sorting;

internal static class PropertyAccessExtensions
{
    /// <summary>
    /// Builds a property selector expression for the given property path.
    /// </summary>
    /// <typeparam name="T">The declaring type of the property.</typeparam>
    /// <param name="property">The dot-separated property path (e.g., "Customer.Name").</param>
    /// <returns>A lambda expression representing the property selector.</returns>
    public static LambdaExpression ToPropertySelector<T>(this string property)
    {
        var type = typeof(T);
        var parameter = Expression.Parameter(type, ParameterNameGenerator.FromType(type));
        Expression expression = parameter;

        foreach (var nestedProperty in property.Split('.'))
        {
            var propertyInfo = type.GetPropertyIgnoreCase(nestedProperty) ??
                throw new ArgumentException(
                    $"No accessible property '{nestedProperty}' defined on type '{type.Name}'.",
                    nameof(property));

            expression = Expression.Property(expression, propertyInfo);
            type = propertyInfo.PropertyType;
        }

        return Expression.Lambda(expression, parameter);
    }

    /// <summary>
    /// Extracts a dot-separated property path from a lambda expression.
    /// </summary>
    /// <param name="property">The property selector expression.</param>
    /// <returns>The extracted property path, or <see langword="null"/> if it cannot be determined.</returns>
    public static string? ToPropertyPath(this LambdaExpression property) =>
        BuildPropertyPath(property.Body);

    private static string? BuildPropertyPath(Expression? expression) =>
        expression switch
        {
            MemberExpression member => $"{BuildPropertyPath(member.Expression)}.{member.Member.Name}".TrimStart('.'),
            UnaryExpression { NodeType: ExpressionType.Convert } unary => BuildPropertyPath(unary.Operand),
            _ => null
        };
}
