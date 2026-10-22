namespace Zift.Pagination.Cursor.Ordering;

internal static class NullableLifting
{
    public static LambdaExpression LiftNavigationPath(LambdaExpression selector)
    {
        if (IsNullable(selector.Body.Type) || !HasReferenceNavigation(selector))
        {
            return selector;
        }

        return Expression.Lambda(ToNullable(selector.Body), selector.Parameters);
    }

    public static Expression ToNullable(Expression valueTypeExpression)
    {
        return Expression.Convert(
            valueTypeExpression,
            typeof(Nullable<>).MakeGenericType(valueTypeExpression.Type));
    }

    public static bool IsNullable(Type type)
    {
        return !type.IsValueType || IsNullableValueType(type);
    }

    private static bool IsNullableValueType(Type type)
    {
        return Nullable.GetUnderlyingType(type) is not null;
    }

    private static bool HasReferenceNavigation(LambdaExpression selector)
    {
        var hasReferenceNavigation = false;
        var current = selector.Body;

        while (current is MemberExpression { Member: PropertyInfo, Expression: { } owner })
        {
            if (IsNullableValueType(owner.Type))
            {
                return false;
            }

            if (owner != selector.Parameters[0] && !owner.Type.IsValueType)
            {
                hasReferenceNavigation = true;
            }

            current = owner;
        }

        return hasReferenceNavigation && current == selector.Parameters[0];
    }
}
