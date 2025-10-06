namespace Zift.Filtering.Dynamic;

internal sealed class FilterExpressionBuilder<T>(FilterCondition condition, FilterOptions? options)
{
    private readonly FilterCondition _condition = condition;
    private readonly FilterOptions _options = options ?? new();

    /// <summary>
    /// Builds a LINQ expression representing the current filter condition.
    /// </summary>
    /// <returns>An expression that evaluates the filter condition against elements of type <typeparamref name="T"/>.</returns>
    public Expression<Func<T, bool>> BuildExpression()
    {
        var parameter = Expression.Parameter(typeof(T), ParameterNameGenerator.FromType<T>());
        var lambdaBody = BuildPathSegmentExpression(parameter, _condition.Property, segmentIndex: 0);

        return Expression.Lambda<Func<T, bool>>(lambdaBody, parameter);
    }

    private Expression BuildPathSegmentExpression(Expression current, PropertyPath propertyPath, int segmentIndex)
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
                : BuildPathSegmentExpression(property, propertyPath, segmentIndex + 1);
        }

        var isFirstSegment = segmentIndex == 0;
        var isNullable = current.Type.IsNullableType();
        var requiresNullGuard = _options.EnableNullGuards && !isFirstSegment && isNullable;

        return requiresNullGuard
            ? NullGuarded(current, segmentExpression)
            : segmentExpression;
    }

    private Expression BuildQuantifierExpression(Expression collection, QuantifierMode quantifier, PropertyPath propertyPath, int segmentIndex)
    {
        var elementType = collection.Type.GetCollectionElementType()!;
        var isLastSegment = segmentIndex == propertyPath.Count - 1;

        var quantifierMethod = quantifier.GetLinqMethod(withPredicate: !isLastSegment).MakeGenericMethod(elementType);
        Expression quantifierExpression;

        if (isLastSegment)
        {
            quantifierExpression = Expression.Call(quantifierMethod, collection);
        }
        else
        {
            var parameter = Expression.Parameter(elementType, ParameterNameGenerator.FromType(elementType));
            var lambdaBody = BuildPathSegmentExpression(parameter, propertyPath, segmentIndex + 1);
            var lambda = Expression.Lambda(lambdaBody, parameter);

            quantifierExpression = Expression.Call(quantifierMethod, collection, lambda);
        }

        return _options.EnableNullGuards
            ? NullGuarded(collection, quantifierExpression)
            : quantifierExpression;
    }

    private Expression BuildProjectionExpression(Expression collection, PropertyPath propertyPath, int segmentIndex)
    {
        var segment = propertyPath[segmentIndex];
        var elementType = collection.Type.GetCollectionElementType()!;

        var projectionMethod = segment.Projection!.Value.GetLinqMethod().MakeGenericMethod(elementType);
        var projectedValue = Expression.Call(projectionMethod, collection);
        var projectionExpression = BuildComparison(projectedValue);

        return _options.EnableNullGuards
            ? NullGuarded(collection, projectionExpression)
            : projectionExpression;
    }

    private Expression BuildComparison(Expression leftOperand)
    {
        var rightOperand = BuildRightHandExpression(leftOperand.Type);

        if (IsCaseInsensitiveComparison(leftOperand))
        {
            leftOperand = ConvertToLowercase(leftOperand);
        }

        var comparison = _condition.Operator.Type.ToComparisonExpression(leftOperand, rightOperand);

        if (!_options.EnableNullGuards || IsDirectComparisonOperator(_condition.Operator.Type))
        {
            return comparison;
        }

        var nullGuard = Expression.AndAlso(IsNonNull(leftOperand), IsNonNull(rightOperand));

        return Expression.Condition(nullGuard, comparison, Expression.Constant(false));
    }

    private Expression BuildRightHandExpression(Type operandType)
    {
        object? normalizedValue;
        Type targetType;

        if (_condition.Operator.Type == ComparisonOperatorType.In)
        {
            var rawValues = (IEnumerable)_condition.Value!;

            normalizedValue = NormalizeValuesToArray(rawValues, operandType);
            targetType = typeof(IEnumerable<>).MakeGenericType(operandType);
        }
        else
        {
            normalizedValue = NormalizeRightHandValue(_condition.Value, operandType);
            targetType = operandType;
        }

        return BuildValueExpression(normalizedValue, targetType);
    }

    private object? NormalizeRightHandValue(object? value, Type operandType)
    {
        if (value is null)
        {
            return null;
        }

        var effectiveType = Nullable.GetUnderlyingType(operandType) ?? operandType;
        var typedValue = EnsureValueOfType(value, effectiveType);

        if (effectiveType == typeof(string) &&
            _condition.Operator.HasModifier("i") &&
            _condition.Operator.Type.SupportedModifiers.Contains("i"))
        {
            return typedValue.ToString()!.ToLower();
        }

        return typedValue;
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

    private static object EnsureValueOfType(object value, Type expectedType)
    {
        if (expectedType.IsInstanceOfType(value))
        {
            return value;
        }

        try
        {
            return Convert.ChangeType(value, expectedType, CultureInfo.InvariantCulture);
        }
        catch (InvalidCastException)
        {
            var typeConverter = TypeDescriptor.GetConverter(expectedType);

            return typeConverter.ConvertFrom(context: null, CultureInfo.InvariantCulture, value)!;
        }
    }

    private Expression BuildValueExpression(object? value, Type expectedType)
    {
        if (!_options.ParameterizeValues)
        {
            return Expression.Constant(value, expectedType);
        }

        LambdaExpression wrappedValue = () => value; // Force EF Core to parameterize the value.

        return Expression.Convert(wrappedValue.Body, expectedType);
    }

    private bool IsCaseInsensitiveComparison(Expression leftOperand) =>
        leftOperand.Type == typeof(string) &&
        _condition.Operator.HasModifier("i") &&
        _condition.Operator.Type.SupportedModifiers.Contains("i");

    private static bool IsDirectComparisonOperator(ComparisonOperatorType @operator) =>
        @operator == ComparisonOperatorType.Equal ||
        @operator == ComparisonOperatorType.NotEqual ||
        @operator == ComparisonOperatorType.GreaterThan ||
        @operator == ComparisonOperatorType.GreaterThanOrEqual ||
        @operator == ComparisonOperatorType.LessThan ||
        @operator == ComparisonOperatorType.LessThanOrEqual;

    private Expression ConvertToLowercase(Expression operand)
    {
        var toLower = Expression.Call(operand, nameof(string.ToLower), Type.EmptyTypes);

        if (!_options.EnableNullGuards)
        {
            return toLower;
        }

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

    private static Expression NullGuarded(Expression subject, Expression innerExpression) =>
        Expression.AndAlso(IsNonNull(subject), innerExpression);

    private static Expression IsNonNull(Expression expression) =>
        expression.Type.IsNullableType()
            ? Expression.NotEqual(expression, Expression.Constant(null, expression.Type))
            : Expression.Constant(true);
}
