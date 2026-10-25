namespace Zift.Querying.ExpressionBuilding;

using Expressions;
using Model;
using Types;

internal sealed class ExpressionBuilder<T>(ExpressionBuilderOptions options)
{
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
            RootNullHandling.AssumeNonNull);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private Expression BuildPredicate(
        PredicateNode node,
        ParameterExpression parameter,
        RootNullHandling rootNullHandling)
    {
        return node switch
        {
            LogicalNode logical =>
                BuildLogical(logical, parameter, rootNullHandling),

            ComparisonNode comparison =>
                BuildComparison(comparison, parameter, rootNullHandling),

            QuantifierNode quantifier =>
                BuildQuantifier(quantifier, parameter, rootNullHandling),

            NotNode not =>
                BuildNot(not, parameter, rootNullHandling),

            _ => throw new NotSupportedException(
                $"Unsupported predicate node: '{node.GetType().Name}'.")
        };
    }

    private Expression BuildLogical(
        LogicalNode node,
        ParameterExpression parameter,
        RootNullHandling rootNullHandling)
    {
        if (node.Terms.Count == 0)
        {
            throw new InvalidOperationException(
                "Logical node must contain at least one term.");
        }

        var expressions = node.Terms
            .Select(term => BuildPredicate(term, parameter, rootNullHandling));

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
        RootNullHandling rootNullHandling)
    {
        var property = BuildPropertyAccess(node.Left, parameter, rootNullHandling);

        if (node.Right is NullLiteral)
        {
            return BuildNullComparison(property, node.Operator);
        }

        if (node.Operator == ComparisonOperator.In)
        {
            return BuildInComparison(property, node.Right);
        }

        return BuildScalarComparison(property, node.Operator, node.Right);
    }

    private Expression BuildQuantifier(
        QuantifierNode node,
        ParameterExpression parameter,
        RootNullHandling rootNullHandling)
    {
        var (collection, reachable) = BuildCollectionAccess(node.Source, parameter, rootNullHandling);

        var elementType = collection.Type.GetCollectionElementType()
            ?? throw new InvalidFilterException(
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
                RootNullHandling.GuardIfNullable);

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

        return WhenReachable(reachable, methodCall, resultWhenUnreachable: node.Kind == QuantifierKind.All);
    }

    private UnaryExpression BuildNot(
        NotNode node,
        ParameterExpression parameter,
        RootNullHandling rootNullHandling)
    {
        var inner = BuildPredicate(node.Inner, parameter, rootNullHandling);

        return Expression.Not(inner);
    }

    private Expression BuildPropertyAccess(
        PropertyNode property,
        ParameterExpression parameter,
        RootNullHandling rootNullHandling)
    {
        return property switch
        {
            PropertyPathNode path =>
                BuildPropertyPathAccess(path, parameter, rootNullHandling),

            ProjectionNode projection =>
                BuildProjectionAccess(projection, parameter, rootNullHandling),

            _ => throw new NotSupportedException(
                $"Unsupported property node: '{property.GetType().Name}'.")
        };
    }

    private Expression BuildPropertyPathAccess(
        PropertyPathNode path,
        Expression root,
        RootNullHandling rootNullHandling)
    {
        var (value, reachable) = Navigate(path, root, rootNullHandling);

        return reachable is null
            ? value
            : PropagateNull(reachable, value);
    }

    private (Expression Collection, Expression? Reachable) BuildCollectionAccess(
        PropertyPathNode path,
        ParameterExpression parameter,
        RootNullHandling rootNullHandling)
    {
        var (collection, reachable) = Navigate(path, parameter, rootNullHandling);

        return (collection, AndNotNull(reachable, collection));
    }

    private (Expression Value, Expression? Reachable) Navigate(
        PropertyPathNode path,
        Expression root,
        RootNullHandling rootNullHandling)
    {
        Expression current = root;
        Expression? reachable = null;

        if (rootNullHandling == RootNullHandling.GuardIfNullable)
        {
            reachable = AndNotNull(reachable, current);
        }

        for (var i = 0; i < path.Segments.Count; i++)
        {
            if (i > 0)
            {
                reachable = AndNotNull(reachable, current);
            }

            current = GetProperty(current, path.Segments[i]);
        }

        return (current, reachable);
    }

    private static MemberExpression GetProperty(Expression owner, string name)
    {
        var property = owner.Type.FindReadableProperty(
            name,
            static (message, inner) => new InvalidFilterException(message, inner));

        return Expression.Property(owner, property);
    }

    private Expression? AndNotNull(Expression? reachable, Expression navigation)
    {
        if (!_options.EnableNullGuards || !navigation.Type.IsNullable())
        {
            return reachable;
        }

        var notNull = Expression.NotEqual(
            navigation,
            Expression.Constant(null, navigation.Type));

        return reachable is null
            ? notNull
            : Expression.AndAlso(reachable, notNull);
    }

    private static ConditionalExpression PropagateNull(Expression reachable, Expression value)
    {
        var type = value.Type.IsNullable()
            ? value.Type
            : typeof(Nullable<>).MakeGenericType(value.Type);

        return Expression.Condition(
            reachable,
            type == value.Type ? value : Expression.Convert(value, type),
            Expression.Constant(null, type));
    }

    private static BinaryExpression IsNull(Expression value)
    {
        return Expression.Equal(value, Expression.Constant(null, value.Type));
    }

    private static Expression WhenReachable(Expression? reachable, Expression body, bool resultWhenUnreachable)
    {
        if (reachable is null)
        {
            return body;
        }

        return resultWhenUnreachable
            ? Expression.OrElse(Expression.Not(reachable), body)
            : Expression.AndAlso(reachable, body);
    }

    private Expression BuildProjectionAccess(
        ProjectionNode projection,
        ParameterExpression parameter,
        RootNullHandling rootNullHandling)
    {
        var (collection, reachable) = BuildCollectionAccess(projection.Source, parameter, rootNullHandling);

        return projection.Projection switch
        {
            CollectionProjection.Count =>
                BuildCountProjection(collection, reachable),

            _ => throw new NotSupportedException(
                $"Unsupported projection: '{projection.Projection}'.")
        };
    }

    private static Expression BuildCountProjection(Expression collection, Expression? reachable)
    {
        var elementType = collection.Type.GetCollectionElementType()
            ?? throw new InvalidFilterException(
                "Count projection requires a collection.");

        var count = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Count),
            [elementType],
            collection);

        return reachable is null
            ? count
            : Expression.Condition(reachable, count, Expression.Constant(0));
    }

    private static Expression BuildNullComparison(
        Expression property,
        ComparisonOperator comparisonOperator)
    {
        if (!property.Type.IsNullable())
        {
            throw new InvalidFilterException(
                $"Null cannot be used for '{property.Type.Name}'.");
        }

        var isNull = IsNull(property);

        return comparisonOperator switch
        {
            ComparisonOperator.Equal => isNull,
            ComparisonOperator.NotEqual => Expression.Not(isNull),

            _ => throw new InvalidFilterException(
                "Only '==' and '!=' operators are supported for null comparisons.")
        };
    }

    private Expression BuildScalarComparison(
        Expression property,
        ComparisonOperator comparisonOperator,
        LiteralNode literal)
    {
        var propertyType = property.Type.GetEffectiveType();

        var comparisonValue = CreateLiteralExpression(literal, property.Type);

        Expression comparison;

        if (propertyType == typeof(string))
        {
            comparison = BuildStringComparison(comparisonOperator, property, comparisonValue);

            if (comparisonOperator is not (ComparisonOperator.Equal or ComparisonOperator.NotEqual))
            {
                comparison = WhenReachable(AndNotNull(null, property), comparison, resultWhenUnreachable: false);
            }
        }
        else
        {
            comparison = BuildBinaryComparison(comparisonOperator, property, comparisonValue, propertyType);
        }

        return comparison;
    }

    private static BinaryExpression BuildBinaryComparison(
        ComparisonOperator comparisonOperator,
        Expression property,
        Expression comparisonValue,
        Type propertyType)
    {
        try
        {
            return comparisonOperator switch
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

                _ => throw new InvalidFilterException(
                    $"Operator '{comparisonOperator}' is not supported for type '{propertyType.Name}'.")
            };
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidFilterException(
                $"Operator '{comparisonOperator}' is not supported for type '{propertyType.Name}'.",
                exception);
        }
    }

    private static Expression BuildStringComparison(
        ComparisonOperator comparisonOperator,
        Expression property,
        Expression comparisonValue)
    {
        return comparisonOperator switch
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
    }

    private Expression BuildInComparison(
        Expression property,
        LiteralNode literal)
    {
        if (literal is not ListLiteral list)
        {
            throw new InvalidFilterException("The 'in' operator requires a list literal.");
        }

        var containsNull = list.Items.Any(i => i is NullLiteral);
        if (containsNull && !property.Type.IsNullable())
        {
            throw new InvalidFilterException(
                $"Null cannot be used for '{property.Type.Name}'.");
        }

        var nonNullItems = list.Items
            .Where(item => item is not NullLiteral)
            .ToList();

        if (nonNullItems.Count == 0)
        {
            return containsNull
                ? IsNull(property)
                : Expression.Constant(false);
        }

        var candidateValues = CreateListLiteralExpression(
            new ListLiteral(nonNullItems),
            property.Type);

        Expression comparison = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [property.Type],
            candidateValues,
            property);

        return containsNull
            ? Expression.OrElse(IsNull(property), comparison)
            : comparison;
    }

    private static object ConvertLiteralValue(LiteralNode literal, Type targetType)
    {
        var effectiveType = targetType.GetEffectiveType();

        return literal switch
        {
            NumberLiteral number =>
                NumericLiteralConverter.Convert(number.Text, effectiveType),

            StringLiteral text =>
                StringLiteralConverter.Convert(text.Value, effectiveType),

            BooleanLiteral boolean =>
                ConvertBooleanValue(boolean.Value, effectiveType),

            ListLiteral => throw new InvalidFilterException(
                "A list literal can only be used with the 'in' operator."),

            _ => throw new NotSupportedException(
                $"Unsupported literal: '{literal.GetType().Name}'.")
        };
    }

    private static bool ConvertBooleanValue(bool value, Type effectiveType)
    {
        return effectiveType.IsInstanceOfType(value)
            ? value
            : throw new InvalidFilterException(
                $"Cannot convert value of type 'Boolean' to '{effectiveType.Name}'.");
    }

    private Expression CreateLiteralExpression(
        LiteralNode literal,
        Type targetType)
    {
        var convertedValue = ConvertLiteralValue(literal, targetType);

        return CreateValueExpression(convertedValue, targetType);
    }

    private Expression CreateListLiteralExpression(
        ListLiteral list,
        Type elementType)
    {
        var array = Array.CreateInstance(elementType, list.Items.Count);

        for (var i = 0; i < list.Items.Count; i++)
        {
            var convertedValue = ConvertLiteralValue(list.Items[i], elementType);

            array.SetValue(convertedValue, i);
        }

        return CreateValueExpression(array, elementType.MakeArrayType());
    }

    private Expression CreateValueExpression(object value, Type targetType)
    {
        return _options.ParameterizeValues
            ? CapturedValue.Create(value, targetType)
            : Expression.Constant(value, targetType);
    }

    private enum RootNullHandling
    {
        AssumeNonNull,
        GuardIfNullable
    }
}
