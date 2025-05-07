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
        var member = Expression.Property(target, segment.Name);
        var isCollection = member.Type.IsCollectionType();

        ValidateCollectionSegment(segment, isCollection);

        Expression segmentExpression;

        if (segment.Projection.HasValue)
        {
            segmentExpression = BuildProjectionExpression(member, propertyPath, segmentIndex);
        }
        else if (isCollection || segment.Quantifier.HasValue)
        {
            segmentExpression = BuildQuantifierExpression(member, segment.Quantifier ?? QuantifierMode.Any, propertyPath, segmentIndex);
        }
        else
        {
            var isLastSegment = segmentIndex == propertyPath.Count - 1;
            if (isLastSegment)
            {
                segmentExpression = BuildComparison(member);
            }
            else
            {
                segmentExpression = BuildSegmentExpression(member, propertyPath, segmentIndex + 1);
            }
        }

        return NullGuarded(target, segmentExpression);
    }

    private Expression BuildQuantifierExpression(Expression collection, QuantifierMode quantifier, PropertyPath propertyPath, int segmentIndex)
    {
        var elementType = collection.Type.GetCollectionElementType()!;
        var isLastSegment = segmentIndex == propertyPath.Count - 1;

        var method = quantifier.GetLinqMethod(withPredicate: !isLastSegment).MakeGenericMethod(elementType);
        Expression methodCall;

        if (isLastSegment)
        {
            methodCall = Expression.Call(method, collection);
        }
        else
        {
            var parameter = Expression.Parameter(elementType, ParameterNameGenerator.FromType(elementType));
            var lambdaBody = BuildSegmentExpression(parameter, propertyPath, segmentIndex + 1);
            var lambda = Expression.Lambda(lambdaBody, parameter);

            methodCall = Expression.Call(method, collection, lambda);
        }

        return NullGuarded(collection, methodCall);
    }

    private Expression BuildProjectionExpression(Expression collection, PropertyPath propertyPath, int segmentIndex)
    {
        var segment = propertyPath[segmentIndex];
        var elementType = collection.Type.GetCollectionElementType()!;

        var method = segment.Projection!.Value.GetLinqMethod().MakeGenericMethod(elementType);
        var methodCall = Expression.Call(method, collection);
        var comparison = BuildComparison(methodCall);

        return NullGuarded(collection, comparison);
    }

    private Expression BuildComparison(Expression leftOperand)
    {
        var rightOperand = BuildValueConversion(leftOperand.Type);

        return ApplyNullSafeComparison(leftOperand, rightOperand);
    }

    private Expression BuildValueConversion(Type targetType)
    {
        if (_condition.Value is not { } value)
        {
            return Expression.Constant(null, targetType);
        }

        var nonNullType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var typedValue = EnsureValueOfType(value, nonNullType);

        LambdaExpression wrappedValue = () => typedValue; // Force EF to parameterize the value.

        return Expression.Convert(wrappedValue.Body, targetType);
    }

    private Expression ApplyNullSafeComparison(Expression leftOperand, Expression rightOperand)
    {
        if (IsCaseInsensitiveStringComparison(leftOperand, rightOperand))
        {
            leftOperand = WrapInNullSafeToLower(leftOperand);
            rightOperand = WrapInNullSafeToLower(rightOperand);
        }

        var comparison = _condition.Operator.Type.ToComparisonExpression(leftOperand, rightOperand);

        var isDirectComparison = _condition.Operator.Type != ComparisonOperatorType.In
            && _condition.Operator.Type != ComparisonOperatorType.Contains
            && _condition.Operator.Type != ComparisonOperatorType.StartsWith
            && _condition.Operator.Type != ComparisonOperatorType.EndsWith;

        if (isDirectComparison)
        {
            return comparison;
        }

        var nullGuard = Expression.AndAlso(IsNonNull(leftOperand), IsNonNull(rightOperand));

        return Expression.Condition(nullGuard, comparison, Expression.Constant(false));
    }

    private bool IsCaseInsensitiveStringComparison(Expression leftOperand, Expression rightOperand)
    {
        return leftOperand.Type == typeof(string)
            && rightOperand.Type == typeof(string)
            && _condition.Operator.HasModifier("i")
            && (_condition.Operator.Type == ComparisonOperatorType.Equal
                || _condition.Operator.Type == ComparisonOperatorType.NotEqual
                || _condition.Operator.Type == ComparisonOperatorType.Contains
                || _condition.Operator.Type == ComparisonOperatorType.StartsWith
                || _condition.Operator.Type == ComparisonOperatorType.EndsWith
                || _condition.Operator.Type == ComparisonOperatorType.In);
    }

    private static Expression WrapInNullSafeToLower(Expression operand)
    {
        var toLower = Expression.Call(operand, nameof(string.ToLower), Type.EmptyTypes);

        return Expression.Condition(
            IsNonNull(operand),
            toLower,
            Expression.Constant(null, typeof(string)));
    }

    private static void ValidateCollectionSegment(PropertyPathSegment segment, bool isCollection)
    {
        if (segment.Projection.HasValue && !isCollection)
        {
            throw new NotSupportedException("Collection projections cannot be applied to scalar properties.");
        }

        if (segment.Quantifier.HasValue && !isCollection)
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

    private static Expression NullGuarded(Expression subject, Expression innerExpression)
    {
        return Expression.AndAlso(IsNonNull(subject), innerExpression);
    }

    private static Expression IsNonNull(Expression expression)
    {
        return Expression.NotEqual(expression, Expression.Constant(null, expression.Type));
    }
}
