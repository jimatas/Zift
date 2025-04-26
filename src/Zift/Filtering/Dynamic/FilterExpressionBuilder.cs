namespace Zift.Filtering.Dynamic;

internal class FilterExpressionBuilder<T>(FilterCondition condition)
{
    private readonly FilterCondition _condition = condition;

    public Expression<Func<T, bool>> BuildExpression()
    {
        var parameter = Expression.Parameter(typeof(T), ParameterNameGenerator.FromType<T>());
        var lambdaBody = BuildSegmentExpression(parameter, _condition.Property, segmentIndex: 0);

        return Expression.Lambda<Func<T, bool>>(lambdaBody, parameter);
    }

    private Expression BuildSegmentExpression(Expression target, PropertyPath propertyPath, int segmentIndex)
    {
        var segment = propertyPath[segmentIndex];
        var isFirstSegment = segmentIndex == 0;
        var isLastSegment = segmentIndex == propertyPath.Count - 1;

        var member = isFirstSegment
            ? Expression.Property(target, segment.Name)
            : BuildNullSafeAccess(target, segment.Name);

        var isCollection = member.Type.IsCollectionType();
        ValidateCollectionSegment(segment, isCollection);

        if (segment.Projection.HasValue)
        {
            return BuildProjectionExpression(member, propertyPath, segmentIndex);
        }

        if (isCollection || segment.Quantifier.HasValue)
        {
            return BuildQuantifierExpression(member, segment.Quantifier ?? QuantifierMode.Any, propertyPath, segmentIndex);
        }

        return isLastSegment
            ? ApplyFinalSegmentNullCheck(BuildComparison(member), member)
            : BuildSegmentExpression(member, propertyPath, segmentIndex + 1);
    }

    private Expression BuildQuantifierExpression(Expression collection, QuantifierMode quantifier, PropertyPath propertyPath, int segmentIndex)
    {
        var isLastSegment = segmentIndex == propertyPath.Count - 1;
        var elementType = collection.Type.GetCollectionElementType()!;
        var method = quantifier.ToLinqMethod(withPredicate: !isLastSegment).MakeGenericMethod(elementType);

        if (isLastSegment)
        {
            return Expression.Call(method, collection);
        }

        var parameter = Expression.Parameter(elementType, ParameterNameGenerator.FromType(elementType));
        var lambdaBody = BuildSegmentExpression(parameter, propertyPath, segmentIndex + 1);
        var lambda = Expression.Lambda(lambdaBody, parameter);

        return Expression.Call(method, collection, lambda);
    }

    private Expression BuildProjectionExpression(Expression collection, PropertyPath propertyPath, int segmentIndex)
    {
        var segment = propertyPath[segmentIndex];
        var elementType = collection.Type.GetCollectionElementType()!;
        var method = segment.Projection!.Value.ToLinqMethod().MakeGenericMethod(elementType);

        return BuildComparison(Expression.Call(method, collection));
    }

    private Expression BuildComparison(Expression leftOperand)
    {
        var rightOperand = BuildValueConversion(leftOperand.Type);

        return ApplyNullSafeComparison(leftOperand, rightOperand);
    }

    private Expression BuildValueConversion(Type targetType)
    {
        if (_condition.Value is null)
        {
            return Expression.Constant(null, targetType);
        }

        var nonNullType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var value = EnsureValueOfType(_condition.Value, nonNullType);

        LambdaExpression wrappedValue = () => value; // Force EF to parameterize the value.

        return Expression.Convert(wrappedValue.Body, targetType);
    }

    private Expression ApplyFinalSegmentNullCheck(Expression condition, Expression expression)
    {
        var isNullSafe = !expression.Type.IsNullableType() || !_condition.Operator.IsImplementedAsMethodCall();
        if (isNullSafe)
        {
            return condition;
        }

        var nullCheck = Expression.NotEqual(expression, Expression.Constant(null, expression.Type));
        var fallback = Expression.Constant(_condition.Operator == ComparisonOperator.NotEqual, typeof(bool));

        return Expression.Condition(nullCheck, condition, fallback);
    }

    private Expression ApplyNullSafeComparison(Expression leftOperand, Expression rightOperand)
    {
        var comparison = _condition.Operator.ToComparisonExpression(leftOperand, rightOperand);

        if (!_condition.Operator.IsImplementedAsMethodCall())
        {
            return comparison;
        }

        var nullCheck = Expression.NotEqual(rightOperand, Expression.Constant(null, leftOperand.Type));
        var fallback = Expression.Constant(false);

        return Expression.Condition(
            nullCheck,
            comparison,
            fallback);
    }

    private static Expression BuildNullSafeAccess(Expression parent, string propertyName)
    {
        var member = Expression.Property(parent, propertyName);
        var nullCheck = Expression.NotEqual(parent, Expression.Constant(null, parent.Type));

        return Expression.Condition(nullCheck, member, Expression.Default(member.Type));
    }

    private static void ValidateCollectionSegment(PropertyPathSegment segment, bool isCollection)
    {
        if (segment.Projection.HasValue && !isCollection)
        {
            throw new NotSupportedException("Collection projections cannot be applied to scalar properties.");
        }

        if ((segment.Quantifier.HasValue || isCollection) && !isCollection)
        {
            throw new NotSupportedException("Quantifier modes cannot be applied to scalar properties.");
        }
    }

    private static object EnsureValueOfType(object value, Type targetType)
    {
        if (!targetType.IsInstanceOfType(value))
        {
            try
            {
                value = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                var typeConverter = TypeDescriptor.GetConverter(targetType);
                value = typeConverter.ConvertFrom(context: null, CultureInfo.InvariantCulture, value)!;
            }
        }

        return value;
    }
}
