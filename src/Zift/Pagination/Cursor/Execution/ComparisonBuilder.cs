namespace Zift.Pagination.Cursor.Execution;

using Ordering;

internal static class ComparisonBuilder
{
    private static readonly MethodInfo _stringCompare =
        typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])!;

    public static Expression BuildComparison(
        Expression key,
        object? cursorValue,
        OrderingDirection direction,
        NullOrdering nullOrdering)
    {
        return BuildComparisonCore(key, cursorValue, direction, nullOrdering, inclusive: false);
    }

    public static Expression? BuildLeadingKeyBound(
        Expression key,
        object? cursorValue,
        OrderingDirection direction,
        NullOrdering nullOrdering)
    {
        var declaredType = key.Type;
        var underlyingType = Nullable.GetUnderlyingType(declaredType);
        var effectiveType = underlyingType ?? declaredType;

        if (cursorValue is null || effectiveType == typeof(bool) ||
            !HasInclusiveOperator(effectiveType, direction))
        {
            return null;
        }

        return BuildComparisonCore(key, cursorValue, direction, nullOrdering, inclusive: true);
    }

    public static Expression BuildEquality(Expression key, object? cursorValue)
    {
        var declaredType = key.Type;
        var underlyingType = Nullable.GetUnderlyingType(declaredType);
        var effectiveType = underlyingType ?? declaredType;
        var isNullable = underlyingType is not null || !declaredType.IsValueType;

        if (cursorValue is null)
        {
            if (!isNullable)
            {
                throw new InvalidCursorException(
                    "The cursor contains a null for a key that cannot be null.");
            }

            return IsNull(key);
        }

        var left = underlyingType is null
            ? key
            : Expression.Property(key, nameof(Nullable<>.Value));

        var right = CaptureValue(cursorValue, effectiveType);

        var equality = effectiveType switch
        {
            var type when type == typeof(string) =>
                Expression.Equal(Expression.Call(_stringCompare, left, right), Expression.Constant(0)),

            var type when type == typeof(bool) || type.IsEnum || HasComparisonOperators(type) =>
                Expression.Equal(left, right),

            _ => BuildComparableEquality(left, right, effectiveType)
        };

        return isNullable
            ? Expression.AndAlso(IsNotNull(key), equality)
            : equality;
    }

    private static Expression BuildComparisonCore(
        Expression key,
        object? cursorValue,
        OrderingDirection direction,
        NullOrdering nullOrdering,
        bool inclusive)
    {
        var declaredType = key.Type;
        var underlyingType = Nullable.GetUnderlyingType(declaredType);
        var effectiveType = underlyingType ?? declaredType;
        var isNullable = underlyingType is not null || !declaredType.IsValueType;

        if (cursorValue is null)
        {
            if (!isNullable)
            {
                throw new InvalidCursorException(
                    "The cursor contains a null for a key that cannot be null.");
            }

            return BuildNullComparison(key, direction, nullOrdering);
        }

        var left = underlyingType is null
            ? key
            : Expression.Property(key, nameof(Nullable<>.Value));

        var comparison = effectiveType switch
        {
            var type when type == typeof(string) =>
                BuildStringComparison(left, CaptureValue(cursorValue, type), direction, inclusive),

            var type when type == typeof(bool) =>
                BuildBooleanComparison(left, CaptureValue(cursorValue, type), direction),

            var type when type.IsEnum =>
                BuildEnumComparison(left, cursorValue, type, direction, inclusive),

            var type when HasComparisonOperators(type) =>
                BuildScalarComparison(left, CaptureValue(cursorValue, type), direction, inclusive),

            _ => BuildComparableComparison(left, CaptureValue(cursorValue, effectiveType), effectiveType, direction, inclusive)
        };

        return isNullable
            ? ApplyNullOrdering(key, comparison, direction, nullOrdering)
            : comparison;
    }

    private static Expression BuildNullComparison(
        Expression key,
        OrderingDirection direction,
        NullOrdering nullOrdering)
    {
        return NullsSortFirst(direction, nullOrdering)
            ? IsNotNull(key)
            : Expression.Constant(false);
    }

    private static BinaryExpression BuildStringComparison(
        Expression left,
        Expression right,
        OrderingDirection direction,
        bool inclusive)
    {
        var compareCall = Expression.Call(_stringCompare, left, right);

        return BuildScalarComparison(compareCall, Expression.Constant(0), direction, inclusive);
    }

    private static BinaryExpression BuildBooleanComparison(
        Expression left,
        Expression right,
        OrderingDirection direction)
    {
        return direction == OrderingDirection.Ascending
            ? Expression.And(left, Expression.Not(right))
            : Expression.And(Expression.Not(left), right);
    }

    private static BinaryExpression BuildEnumComparison(
        Expression left,
        object value,
        Type enumType,
        OrderingDirection direction,
        bool inclusive)
    {
        var underlyingType = Enum.GetUnderlyingType(enumType);
        var right = CaptureValue(Convert.ChangeType(value, underlyingType), underlyingType);

        return BuildScalarComparison(Expression.Convert(left, underlyingType), right, direction, inclusive);
    }

    private static BinaryExpression BuildScalarComparison(
        Expression left,
        Expression right,
        OrderingDirection direction,
        bool inclusive)
    {
        return (direction, inclusive) switch
        {
            (OrderingDirection.Ascending, false) => Expression.GreaterThan(left, right),
            (OrderingDirection.Ascending, true) => Expression.GreaterThanOrEqual(left, right),
            (_, false) => Expression.LessThan(left, right),
            _ => Expression.LessThanOrEqual(left, right)
        };
    }

    private static BinaryExpression BuildComparableComparison(
        Expression left,
        Expression right,
        Type type,
        OrderingDirection direction,
        bool inclusive)
    {
        var compareCall = BuildCompareToCall(left, right, type)
            ?? throw new NotSupportedException(
                $"Ordering by '{type.Name}' is not supported for cursor pagination: the type has neither comparison operators nor IComparable<{type.Name}>.");

        return BuildScalarComparison(compareCall, Expression.Constant(0), direction, inclusive);
    }

    private static BinaryExpression BuildComparableEquality(Expression left, Expression right, Type type)
    {
        var compareCall = BuildCompareToCall(left, right, type)
            ?? throw new NotSupportedException(
                $"Ordering by '{type.Name}' is not supported for cursor pagination.");

        return Expression.Equal(compareCall, Expression.Constant(0));
    }

    private static BinaryExpression ApplyNullOrdering(
        Expression key,
        Expression comparison,
        OrderingDirection direction,
        NullOrdering nullOrdering)
    {
        var isNull = IsNull(key);
        var isNotNull = IsNotNull(key);

        return NullsSortFirst(direction, nullOrdering)
            ? Expression.AndAlso(isNotNull, comparison)
            : Expression.OrElse(isNull, Expression.AndAlso(isNotNull, comparison));
    }

    private static bool NullsSortFirst(OrderingDirection direction, NullOrdering nullOrdering)
    {
        return direction == OrderingDirection.Ascending
            ? nullOrdering == NullOrdering.NullsCompareLower
            : nullOrdering == NullOrdering.NullsCompareHigher;
    }

    private static UnaryExpression CaptureValue(object value, Type type)
    {
        Expression<Func<object>> accessor = () => value;

        return Expression.Convert(accessor.Body, type);
    }

    private static MethodCallExpression? BuildCompareToCall(Expression left, Expression right, Type type)
    {
        var compareTo = type.GetMethod(nameof(IComparable.CompareTo), [type]);
        if (compareTo is null || !typeof(IComparable<>).MakeGenericType(type).IsAssignableFrom(type))
        {
            return null;
        }

        return Expression.Call(left, compareTo, right);
    }

    private static bool HasComparisonOperators(Type type)
    {
        return (type.IsPrimitive && type != typeof(bool)) ||
            type == typeof(decimal) ||
            type.GetMethod("op_GreaterThan", BindingFlags.Public | BindingFlags.Static, [type, type]) is not null;
    }

    private static bool HasInclusiveOperator(Type type, OrderingDirection direction)
    {
        if (!HasComparisonOperators(type) || type.IsPrimitive || type == typeof(decimal))
        {
            return true;
        }

        var operatorName = direction == OrderingDirection.Ascending
            ? "op_GreaterThanOrEqual"
            : "op_LessThanOrEqual";

        return type.GetMethod(operatorName, BindingFlags.Public | BindingFlags.Static, [type, type]) is not null;
    }

    private static BinaryExpression IsNull(Expression key)
    {
        return Expression.Equal(key, Expression.Constant(null, key.Type));
    }

    private static BinaryExpression IsNotNull(Expression key)
    {
        return Expression.NotEqual(key, Expression.Constant(null, key.Type));
    }
}
