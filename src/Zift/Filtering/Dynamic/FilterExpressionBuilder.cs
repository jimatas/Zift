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

    private Expression BuildSegmentExpression(Expression current, PropertyPath propertyPath, int segmentIndex)
    {
        var segment = propertyPath[segmentIndex];
        var property = Expression.Property(current, segment.Name);
        var isCollection = property.Type.IsCollectionType();

        ValidateCollectionSegment(segment, isCollection);

        Expression segmentExpression;

        if (segment.Projection.HasValue)
        {
            segmentExpression = BuildProjectionExpression(property, propertyPath, segmentIndex);
        }
        else if (isCollection || segment.Quantifier.HasValue)
        {
            segmentExpression = BuildQuantifierExpression(property, segment.Quantifier ?? QuantifierMode.Any, propertyPath, segmentIndex);
        }
        else
        {
            var isLastSegment = segmentIndex == propertyPath.Count - 1;

            segmentExpression = isLastSegment
                ? BuildComparison(property)
                : BuildSegmentExpression(property, propertyPath, segmentIndex + 1);
        }

        var isFirstSegment = segmentIndex == 0;

        return isFirstSegment
            ? segmentExpression // No null guard needed for the root parameter.
            : NullGuarded(current, segmentExpression);
    }

    private Expression BuildQuantifierExpression(Expression collection, QuantifierMode quantifier, PropertyPath propertyPath, int segmentIndex)
    {
        var elementType = collection.Type.GetCollectionElementType()!;
        var isLastSegment = segmentIndex == propertyPath.Count - 1;

        var quantifierMethod = quantifier.GetLinqMethod(withPredicate: !isLastSegment).MakeGenericMethod(elementType);
        Expression methodCall;

        if (isLastSegment)
        {
            methodCall = Expression.Call(quantifierMethod, collection);
        }
        else
        {
            var parameter = Expression.Parameter(elementType, ParameterNameGenerator.FromType(elementType));
            var lambdaBody = BuildSegmentExpression(parameter, propertyPath, segmentIndex + 1);
            var lambda = Expression.Lambda(lambdaBody, parameter);

            methodCall = Expression.Call(quantifierMethod, collection, lambda);
        }

        return NullGuarded(collection, methodCall);
    }

    private Expression BuildProjectionExpression(Expression collection, PropertyPath propertyPath, int segmentIndex)
    {
        var segment = propertyPath[segmentIndex];
        var elementType = collection.Type.GetCollectionElementType()!;

        var projectionMethod = segment.Projection!.Value.GetLinqMethod().MakeGenericMethod(elementType);
        var methodCall = Expression.Call(projectionMethod, collection);
        var comparison = BuildComparison(methodCall);

        return NullGuarded(collection, comparison);
    }

    private Expression BuildComparison(Expression leftOperand)
    {
        var rightOperand = BuildRightHandExpression(leftOperand.Type);

        return ApplyNullSafeComparison(leftOperand, rightOperand);
    }

    private Expression BuildRightHandExpression(Type operandType)
    {
        object? normalizedValue;
        Type parameterType;

        if (_condition.Operator.Type == ComparisonOperatorType.In)
        {
            var rawValues = (IEnumerable)_condition.Value!;

            normalizedValue = NormalizeValuesToArray(rawValues, operandType);
            parameterType = typeof(IEnumerable<>).MakeGenericType(operandType);
        }
        else
        {
            normalizedValue = NormalizeRightHandValue(_condition.Value, operandType);
            parameterType = operandType;
        }

        return ConstantAsParameter(normalizedValue, parameterType);
    }

    private object? NormalizeRightHandValue(object? value, Type operandType)
    {
        if (value is null)
        {
            return null;
        }

        var effectiveType = Nullable.GetUnderlyingType(operandType) ?? operandType;
        var typedValue = EnsureValueOfType(value, effectiveType);

        if (effectiveType == typeof(string)
            && _condition.Operator.HasModifier("i")
            && _condition.Operator.Type.SupportedModifiers.Contains("i"))
        {
            return typedValue.ToString()!.ToLower();
        }

        return typedValue;
    }

    private static object EnsureValueOfType(object value, Type expectedType)
    {
        if (!expectedType.IsInstanceOfType(value))
        {
            try
            {
                value = Convert.ChangeType(value, expectedType, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                var typeConverter = TypeDescriptor.GetConverter(expectedType);

                value = typeConverter.ConvertFrom(context: null, CultureInfo.InvariantCulture, value)!;
            }
        }

        return value;
    }

    private Array NormalizeValuesToArray(IEnumerable rawValues, Type elementType)
    {
        var normalizedValues = rawValues.Cast<object?>()
            .Select(value => NormalizeRightHandValue(value, elementType))
            .ToArray();

        var array = Array.CreateInstance(elementType, normalizedValues.Length);
        normalizedValues.CopyTo(array, 0);

        return array;
    }

    private static Expression ConstantAsParameter(object? value, Type parameterType)
    {
        LambdaExpression wrappedValue = () => value; // Wrap in a lambda to force parameterization by EF Core.

        return Expression.Convert(wrappedValue.Body, parameterType);
    }

    private Expression ApplyNullSafeComparison(Expression leftOperand, Expression rightOperand)
    {
        if (IsCaseInsensitiveStringComparison(leftOperand))
        {
            leftOperand = WrapInNullSafeToLower(leftOperand);
        }

        var comparison = _condition.Operator.Type.ToComparisonExpression(leftOperand, rightOperand);

        if (IsDirectComparisonOperator(_condition.Operator.Type))
        {
            return comparison;
        }

        var nullGuard = Expression.AndAlso(IsNonNull(leftOperand), IsNonNull(rightOperand));

        return Expression.Condition(nullGuard, comparison, Expression.Constant(false));
    }

    private bool IsCaseInsensitiveStringComparison(Expression leftOperand)
    {
        return leftOperand.Type == typeof(string)
            && _condition.Operator.HasModifier("i")
            && _condition.Operator.Type.SupportedModifiers.Contains("i");
    }

    private static bool IsDirectComparisonOperator(ComparisonOperatorType @operator)
    {
        return @operator == ComparisonOperatorType.Equal
            || @operator == ComparisonOperatorType.NotEqual
            || @operator == ComparisonOperatorType.GreaterThan
            || @operator == ComparisonOperatorType.GreaterThanOrEqual
            || @operator == ComparisonOperatorType.LessThan
            || @operator == ComparisonOperatorType.LessThanOrEqual;
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

    private static Expression NullGuarded(Expression subject, Expression innerExpression)
    {
        return Expression.AndAlso(IsNonNull(subject), innerExpression);
    }

    private static Expression IsNonNull(Expression expression)
    {
        return expression.Type.IsNullableType()
            ? Expression.NotEqual(expression, Expression.Constant(null, expression.Type))
            : Expression.Constant(true);
    }
}
