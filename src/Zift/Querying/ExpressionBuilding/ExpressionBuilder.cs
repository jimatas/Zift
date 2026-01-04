namespace Zift.Querying.ExpressionBuilding;

using Expressions;
using Model;
using Types;

internal sealed class ExpressionBuilder<T>(ExpressionBuilderOptions options)
{
    private static readonly Dictionary<Type, Func<string, object>> _stringParsers = new()
    {
        [typeof(DateTime)] = s =>
            DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),

        [typeof(DateTimeOffset)] = s =>
            DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),

        [typeof(Guid)] = s => Guid.Parse(s),

        [typeof(DateOnly)] = s =>
            DateOnly.Parse(s, CultureInfo.InvariantCulture),

        [typeof(TimeOnly)] = s =>
            TimeOnly.Parse(s, CultureInfo.InvariantCulture)
    };

    private static readonly MethodInfo _stringCompareTo =
        typeof(string).GetMethod(
            nameof(string.CompareTo),
            [typeof(string)])!;

    private readonly ExpressionBuilderOptions _options = options;

    public Expression<Func<T, bool>> Build(PredicateNode node)
    {
        var parameter = Expression.Parameter(
            typeof(T),
            ParameterName.FromType<T>());

        var body = BuildPredicate(
            node,
            parameter,
            PropertyAccessContext.PredicateRoot);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private Expression BuildPredicate(
        PredicateNode node,
        ParameterExpression parameter,
        PropertyAccessContext context) =>
        node switch
        {
            LogicalNode logical =>
                BuildLogical(logical, parameter, context),

            ComparisonNode comparison =>
                BuildComparison(comparison, parameter, context),

            QuantifierNode quantifier =>
                BuildQuantifier(quantifier, parameter, context),

            NotNode not =>
                BuildNot(not, parameter, context),

            _ => throw new NotSupportedException(
                $"Unsupported predicate node: '{node.GetType().Name}'.")
        };

    private Expression BuildLogical(
        LogicalNode node,
        ParameterExpression parameter,
        PropertyAccessContext context)
    {
        if (node.Terms.Count == 0)
        {
            throw new InvalidOperationException(
                "Logical node must contain at least one term.");
        }

        var expressions = node.Terms
            .Select(term => BuildPredicate(term, parameter, context));

        return node.Operator switch
        {
            LogicalOperator.And =>
                expressions.Aggregate(Expression.AndAlso),

            LogicalOperator.Or =>
                expressions.Aggregate(Expression.OrElse),

            _ => throw new NotSupportedException(
                $"Unsupported logical operator: '{node.Operator}'.")
        };
    }

    private Expression BuildComparison(
        ComparisonNode node,
        ParameterExpression parameter,
        PropertyAccessContext context)
    {
        var propertyAccess = BuildPropertyAccess(node.Left, parameter, context);

        if (node.Right is NullLiteral)
        {
            return BuildNullComparison(propertyAccess, node.Operator);
        }

        if (node.Operator == ComparisonOperator.In)
        {
            return BuildInComparison(propertyAccess, node.Right);
        }

        return BuildScalarComparison(propertyAccess, node.Operator, node.Right);
    }

    private Expression BuildQuantifier(
        QuantifierNode node,
        ParameterExpression parameter,
        PropertyAccessContext context)
    {
        var (collection, nullGuard) = BuildPropertyAccess(node.Source, parameter, context);

        var elementType = collection.Type.GetCollectionElementType()
            ?? throw new InvalidOperationException(
                "Quantifiers can only be applied to collections.");

        MethodCallExpression methodCall;

        if (node.Predicate is null)
        {
            if (node.Kind != QuantifierKind.Any)
            {
                throw new InvalidOperationException(
                    $"Quantifier '{node.Kind}' requires a predicate.");
            }

            methodCall = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                [elementType],
                collection);
        }
        else
        {
            var elementParameter = Expression.Parameter(
                elementType,
                ParameterName.FromType(elementType));

            var predicateBody = BuildPredicate(
                node.Predicate,
                elementParameter,
                PropertyAccessContext.QuantifierElement);

            var predicate = Expression.Lambda(predicateBody, elementParameter);

            var methodName = node.Kind == QuantifierKind.Any
                ? nameof(Enumerable.Any)
                : nameof(Enumerable.All);

            methodCall = Expression.Call(
                typeof(Enumerable),
                methodName,
                [elementType],
                collection,
                predicate);
        }

        if (nullGuard is null)
        {
            return methodCall;
        }

        return node.Kind == QuantifierKind.All
            ? Expression.OrElse(Expression.Not(nullGuard), methodCall)
            : Expression.AndAlso(nullGuard, methodCall);
    }

    private UnaryExpression BuildNot(
        NotNode node,
        ParameterExpression parameter,
        PropertyAccessContext context)
    {
        var inner = BuildPredicate(node.Inner, parameter, context);

        return Expression.Not(inner);
    }

    private GuardedPropertyAccess BuildPropertyAccess(
        PropertyNode property,
        ParameterExpression parameter,
        PropertyAccessContext context) =>
        property switch
        {
            PropertyPathNode path =>
                BuildPropertyPathAccess(path, parameter, context),

            ProjectionNode projection =>
                BuildProjectionAccess(projection, parameter, context),

            _ => throw new NotSupportedException(
                $"Unsupported property node: '{property.GetType().Name}'.")
        };

    private GuardedPropertyAccess BuildPropertyPathAccess(
        PropertyPathNode path,
        Expression root,
        PropertyAccessContext context) =>
        GuardedPropertyAccessBuilder.Build(
            root,
            path.Segments,
            _options.EnableNullGuards,
            guardRoot: context == PropertyAccessContext.QuantifierElement);

    private GuardedPropertyAccess BuildProjectionAccess(
        ProjectionNode projection,
        ParameterExpression parameter,
        PropertyAccessContext context)
    {
        var (collection, nullGuard) = BuildPropertyAccess(projection.Source, parameter, context);

        var projected = projection.Projection switch
        {
            CollectionProjection.Count =>
                BuildCountProjection(collection),

            _ => throw new NotSupportedException(
                $"Unsupported projection: '{projection.Projection}'.")
        };

        return new GuardedPropertyAccess(projected, nullGuard);
    }

    private static MethodCallExpression BuildCountProjection(Expression collection)
    {
        var elementType = collection.Type.GetCollectionElementType()
            ?? throw new InvalidOperationException(
                "Count projection requires a collection.");

        return Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Count),
            [elementType],
            collection);
    }

    private static Expression BuildNullComparison(
        GuardedPropertyAccess propertyAccess,
        ComparisonOperator comparisonOperator)
    {
        var property = propertyAccess.Value;

        if (!property.Type.IsNullable())
        {
            throw new InvalidOperationException(
                $"Null cannot be used for '{property.Type.Name}'.");
        }

        var isNull = Expression.Equal(
            property,
            Expression.Constant(null, property.Type));

        Expression comparison = comparisonOperator switch
        {
            ComparisonOperator.Equal => isNull,
            ComparisonOperator.NotEqual => Expression.Not(isNull),

            _ => throw new InvalidOperationException(
                "Only '==' and '!=' operators are supported for null comparisons.")
        };

        if (propertyAccess.NullGuard is not { } nullGuard)
        {
            return comparison;
        }

        return comparisonOperator == ComparisonOperator.Equal
            ? Expression.OrElse(Expression.Not(nullGuard), comparison)
            : Expression.AndAlso(nullGuard, comparison);
    }

    private Expression BuildScalarComparison(
        GuardedPropertyAccess propertyAccess,
        ComparisonOperator comparisonOperator,
        LiteralNode literal)
    {
        var property = propertyAccess.Value;
        var propertyType = property.Type.GetEffectiveType();

        var comparisonValue = CreateLiteralExpression(literal, property.Type);

        Expression comparison;

        if (propertyType == typeof(string))
        {
            comparison = BuildStringComparison(comparisonOperator, property, comparisonValue);
        }
        else
        {
            comparison = comparisonOperator switch
            {
                ComparisonOperator.Equal =>
                    Expression.Equal(property, comparisonValue),

                ComparisonOperator.NotEqual =>
                    Expression.NotEqual(property, comparisonValue),

                ComparisonOperator.LessThan when propertyType.IsOrderable() =>
                    Expression.LessThan(property, comparisonValue),

                ComparisonOperator.LessThanOrEqual when propertyType.IsOrderable() =>
                    Expression.LessThanOrEqual(property, comparisonValue),

                ComparisonOperator.GreaterThan when propertyType.IsOrderable() =>
                    Expression.GreaterThan(property, comparisonValue),

                ComparisonOperator.GreaterThanOrEqual when propertyType.IsOrderable() =>
                    Expression.GreaterThanOrEqual(property, comparisonValue),

                _ => throw new NotSupportedException(
                    $"Operator '{comparisonOperator}' is not supported for type '{propertyType.Name}'.")
            };
        }

        return propertyAccess.NullGuard is { } nullGuard
            ? Expression.AndAlso(nullGuard, comparison)
            : comparison;
    }

    private static Expression BuildStringComparison(
        ComparisonOperator comparisonOperator,
        Expression property,
        Expression comparisonValue) =>
        comparisonOperator switch
        {
            ComparisonOperator.Contains =>
                Expression.Call(
                    property,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    comparisonValue),

            ComparisonOperator.StartsWith =>
                Expression.Call(
                    property,
                    nameof(string.StartsWith),
                    Type.EmptyTypes,
                    comparisonValue),

            ComparisonOperator.EndsWith =>
                Expression.Call(
                    property,
                    nameof(string.EndsWith),
                    Type.EmptyTypes,
                    comparisonValue),

            ComparisonOperator.GreaterThan =>
                Expression.GreaterThan(
                    Expression.Call(property, _stringCompareTo, comparisonValue),
                    Expression.Constant(0)),

            ComparisonOperator.GreaterThanOrEqual =>
                Expression.GreaterThanOrEqual(
                    Expression.Call(property, _stringCompareTo, comparisonValue),
                    Expression.Constant(0)),

            ComparisonOperator.LessThan =>
                Expression.LessThan(
                    Expression.Call(property, _stringCompareTo, comparisonValue),
                    Expression.Constant(0)),

            ComparisonOperator.LessThanOrEqual =>
                Expression.LessThanOrEqual(
                    Expression.Call(property, _stringCompareTo, comparisonValue),
                    Expression.Constant(0)),

            ComparisonOperator.Equal =>
                Expression.Equal(property, comparisonValue),

            ComparisonOperator.NotEqual =>
                Expression.NotEqual(property, comparisonValue),

            _ => throw new NotSupportedException(
                $"Operator '{comparisonOperator}' is not supported for string.")
        };

    private Expression BuildInComparison(
        GuardedPropertyAccess propertyAccess,
        LiteralNode literal)
    {
        if (literal is not ListLiteral list)
        {
            throw new NotSupportedException("The 'in' operator requires a list literal.");
        }

        var property = propertyAccess.Value;

        var containsNull = list.Items.Any(i => i is NullLiteral);
        if (containsNull && !property.Type.IsNullable())
        {
            throw new InvalidOperationException(
                $"Null cannot be used for '{property.Type.Name}'.");
        }

        var nonNullItems = list.Items
            .Where(item => item is not NullLiteral)
            .ToList();

        Expression comparison;

        if (nonNullItems.Count == 0)
        {
            comparison = property.Type.IsNullable()
                ? Expression.Equal(
                    property,
                    Expression.Constant(null, property.Type))
                : Expression.Constant(false);
        }
        else
        {
            var candidateValues = CreateListLiteralExpression(
                new ListLiteral(nonNullItems),
                property.Type);

            comparison = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                [property.Type],
                candidateValues,
                property);

            if (containsNull)
            {
                comparison = Expression.OrElse(
                    Expression.Equal(
                        property,
                        Expression.Constant(null, property.Type)),
                    comparison);
            }
        }

        if (propertyAccess.NullGuard is not { } nullGuard)
        {
            return comparison;
        }

        return containsNull
            ? Expression.OrElse(Expression.Not(nullGuard), comparison)
            : Expression.AndAlso(nullGuard, comparison);
    }

    private static object ExtractLiteralValue(LiteralNode literal) =>
        literal switch
        {
            NumberLiteral n => n.Value,
            StringLiteral s => s.Value,
            BooleanLiteral b => b.Value,

            _ => throw new NotSupportedException(
                $"Unsupported literal: '{literal.GetType().Name}'.")
        };

    private static object ConvertLiteralValue(object value, Type targetType)
    {
        var effectiveType = targetType.GetEffectiveType();

        if (effectiveType.IsInstanceOfType(value))
        {
            return value;
        }

        if (value.GetType().IsNumeric() && effectiveType.IsNumeric())
        {
            return Convert.ChangeType(value, effectiveType, CultureInfo.InvariantCulture);
        }

        if (value is string s)
        {
            if (_stringParsers.TryGetValue(effectiveType, out var parser))
            {
                return parser(s);
            }

            if (effectiveType.IsEnum)
            {
                return Enum.Parse(effectiveType, s, ignoreCase: false);
            }

            throw new InvalidOperationException(
                $"String values cannot be converted to '{effectiveType.Name}'.");
        }

        throw new InvalidOperationException(
            $"Cannot convert value of type '{value.GetType().Name}' to '{effectiveType.Name}'.");
    }

    private Expression CreateLiteralExpression(
        LiteralNode literal,
        Type targetType)
    {
        var rawValue = ExtractLiteralValue(literal);
        var convertedValue = ConvertLiteralValue(rawValue, targetType);

        return CreateValueExpression(convertedValue, targetType);
    }

    private Expression CreateListLiteralExpression(
        ListLiteral list,
        Type elementType)
    {
        var array = Array.CreateInstance(elementType, list.Items.Count);

        for (var i = 0; i < list.Items.Count; i++)
        {
            var rawValue = ExtractLiteralValue(list.Items[i]);
            var convertedValue = ConvertLiteralValue(rawValue, elementType);

            array.SetValue(convertedValue, i);
        }

        return CreateValueExpression(array, elementType.MakeArrayType());
    }

    private Expression CreateValueExpression(object value, Type targetType)
    {
        if (!_options.ParameterizeValues)
        {
            return Expression.Constant(value, targetType);
        }

        LambdaExpression wrapper = () => value;

        return Expression.Convert(wrapper.Body, targetType);
    }

    private enum PropertyAccessContext
    {
        PredicateRoot,
        QuantifierElement
    }
}
