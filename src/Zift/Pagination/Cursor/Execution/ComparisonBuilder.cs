namespace Zift.Pagination.Cursor.Execution;

using Ordering;

internal static class ComparisonBuilder
{
    private static readonly Dictionary<Type, MethodInfo> _compareMethods = new()
    {
        [typeof(string)] =
            typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])!,

        [typeof(bool)] =
            typeof(bool).GetMethod(nameof(bool.CompareTo), [typeof(bool)])!
    };

    public static Expression BuildComparison(
        Expression property,
        object? value,
        OrderingDirection direction)
    {
        var declaredType = property.Type;
        var underlyingType = Nullable.GetUnderlyingType(declaredType);
        var effectiveType = underlyingType ?? declaredType;

        Expression leftOperand;
        Expression rightOperand;

        var isNullable = underlyingType is not null;
        if (isNullable)
        {
            if (value is null)
            {
                leftOperand = property;
                rightOperand = Expression.Constant(null, declaredType);
            }
            else
            {
                leftOperand = Expression.Property(property, nameof(Nullable<>.Value));
                rightOperand = Expression.Constant(value, underlyingType!);
            }
        }
        else
        {
            leftOperand = property;
            rightOperand = Expression.Constant(value, declaredType);
        }

        if (value is null && isNullable)
        {
            return BuildNullComparison(property, direction);
        }

        var comparison = effectiveType switch
        {
            var type when type == typeof(string) =>
                BuildStringComparison(leftOperand, rightOperand, direction),

            var type when type == typeof(bool) =>
                BuildBooleanComparison(leftOperand, rightOperand, direction),

            var type when type.IsEnum =>
                BuildEnumComparison(leftOperand, rightOperand, type, direction),

            _ => BuildScalarComparison(leftOperand, rightOperand, direction)
        };

        return isNullable
            ? ApplyNullOrdering(property, comparison, direction)
            : comparison;
    }

    public static Expression BuildEquality(Expression property, object? value) =>
        Expression.Equal(property, Expression.Constant(value, property.Type));

    private static Expression BuildNullComparison(
        Expression property,
        OrderingDirection direction)
    {
        var isNotNull = Expression.NotEqual(
            property,
            Expression.Constant(null, property.Type));

        return direction == OrderingDirection.Ascending
            ? isNotNull
            : Expression.Constant(false);
    }

    private static BinaryExpression BuildStringComparison(
        Expression leftOperand,
        Expression rightOperand,
        OrderingDirection direction)
    {
        var compareCall = Expression.Call(_compareMethods[typeof(string)], leftOperand, rightOperand);

        return direction == OrderingDirection.Ascending
            ? Expression.GreaterThan(compareCall, Expression.Constant(0))
            : Expression.LessThan(compareCall, Expression.Constant(0));
    }

    private static BinaryExpression BuildBooleanComparison(
        Expression leftOperand,
        Expression rightOperand,
        OrderingDirection direction)
    {
        var compareCall = Expression.Call(leftOperand, _compareMethods[typeof(bool)], rightOperand);

        return direction == OrderingDirection.Ascending
            ? Expression.GreaterThan(compareCall, Expression.Constant(0))
            : Expression.LessThan(compareCall, Expression.Constant(0));
    }

    private static BinaryExpression BuildEnumComparison(
        Expression leftOperand,
        Expression rightOperand,
        Type enumType,
        OrderingDirection direction)
    {
        var underlyingType = Enum.GetUnderlyingType(enumType);

        return BuildScalarComparison(
           Expression.Convert(leftOperand, underlyingType),
           Expression.Convert(rightOperand, underlyingType),
           direction);
    }

    private static BinaryExpression BuildScalarComparison(
        Expression leftOperand,
        Expression rightOperand,
        OrderingDirection direction)
    {
        return direction == OrderingDirection.Ascending
            ? Expression.GreaterThan(leftOperand, rightOperand)
            : Expression.LessThan(leftOperand, rightOperand);
    }

    private static BinaryExpression ApplyNullOrdering(
        Expression property,
        Expression comparison,
        OrderingDirection direction)
    {
        var isNull = Expression.Equal(
            property,
            Expression.Constant(null, property.Type));

        var isNotNull = Expression.NotEqual(
            property,
            Expression.Constant(null, property.Type));

        return direction == OrderingDirection.Ascending
            ? Expression.AndAlso(isNotNull, comparison)
            : Expression.OrElse(
                isNull,
                Expression.AndAlso(isNotNull, comparison));
    }
}
