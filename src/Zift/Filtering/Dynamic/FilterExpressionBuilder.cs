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
            segmentExpression = isLastSegment
                ? BuildComparison(member)
                : BuildSegmentExpression(member, propertyPath, segmentIndex + 1);
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
        if (_condition.Operator.Type == ComparisonOperatorType.In)
        {
            return BuildInComparison(leftOperand);
        }

        var rightOperand = BuildRightHandExpression(leftOperand.Type);

        return ApplyNullSafeComparison(leftOperand, rightOperand);
    }

    private Expression BuildInComparison(Expression leftOperand)
    {
        var elementType = leftOperand.Type;
        var rawValues = (IEnumerable)_condition.Value!;
        var normalizedValues = NormalizeValuesToArray(rawValues, elementType);
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
        var rightOperand = ConstantAsParameter(normalizedValues, enumerableType);

        if (IsCaseInsensitiveStringComparison(leftOperand))
        {
            leftOperand = WrapInNullSafeToLower(leftOperand);
        }

        var comparison = ComparisonOperatorType.In.ToComparisonExpression(leftOperand, rightOperand);

        return Expression.Condition(
            IsNonNull(leftOperand),
            comparison,
            Expression.Constant(false));
    }

    private Expression BuildRightHandExpression(Type targetType)
    {
        var normalizedValue = NormalizeRightHandValue(_condition.Value, targetType);

        return ConstantAsParameter(normalizedValue, targetType);
    }

    private object? NormalizeRightHandValue(object? value, Type targetType)
    {
        if (value is null)
        {
            return null;
        }

        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        var typedValue = EnsureValueOfType(value, targetType);

        if (targetType == typeof(string)
            && _condition.Operator.HasModifier("i")
            && _condition.Operator.Type.SupportedModifiers.Contains("i"))
        {
            return typedValue.ToString()!.ToLowerInvariant();
        }

        return typedValue;
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

    private Array NormalizeValuesToArray(IEnumerable values, Type elementType)
    {
        var normalizedValues = values.Cast<object?>()
            .Select(value => NormalizeRightHandValue(value, elementType))
            .ToArray();

        var array = Array.CreateInstance(elementType, normalizedValues.Length);
        normalizedValues.CopyTo(array, 0);

        return array;
    }

    private static Expression ConstantAsParameter(object? value, Type targetType)
    {
        LambdaExpression wrapper = () => value; // Wrap in a lambda to force parameterization by EF Core.

        return Expression.Convert(wrapper.Body, targetType);
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
        return @operator != ComparisonOperatorType.In
            && @operator != ComparisonOperatorType.Contains
            && @operator != ComparisonOperatorType.StartsWith
            && @operator != ComparisonOperatorType.EndsWith;
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
