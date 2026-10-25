namespace Zift.Pagination.Cursor.Ordering;

using Expressions;
using Types;

internal static class PropertyPathResolver
{
    public static LambdaExpression Resolve<T>(string propertyPath, bool nullGuards)
    {
        var parameter = Expression.Parameter(typeof(T), ParameterName.FromType<T>());

        Expression current = parameter;
        Expression? guard = null;
        var hasNullableNavigation = false;

        foreach (var segment in propertyPath.Split('.'))
        {
            if (current != parameter && current.Type.IsNullable())
            {
                hasNullableNavigation = true;

                if (nullGuards)
                {
                    var notNull = Expression.NotEqual(
                        current,
                        Expression.Constant(null, current.Type));

                    guard = guard is null
                        ? notNull
                        : Expression.AndAlso(guard, notNull);
                }
            }

            current = ResolveProperty(current, segment);
        }

        if (hasNullableNavigation && !current.Type.IsNullable())
        {
            current = NullableLifting.ToNullable(current);
        }

        var body = guard is null
            ? current
            : Expression.Condition(
                guard,
                current,
                Expression.Default(current.Type));

        return Expression.Lambda(body, parameter);
    }

    private static MemberExpression ResolveProperty(Expression instance, string name)
    {
        var property = instance.Type.FindReadableProperty(
            name,
            static (message, inner) => new InvalidOrderingClauseException(message, inner));

        return Expression.Property(instance, property);
    }
}
