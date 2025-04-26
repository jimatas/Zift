namespace Zift.Filtering.Dynamic;

internal class GroupedFilterExpressionBuilder<T>(FilterGroup group)
{
    private readonly FilterGroup _group = group;

    public Expression<Func<T, bool>> BuildExpression()
    {
        var parameter = Expression.Parameter(typeof(T), ParameterNameGenerator.FromType<T>());
        var lambdaBody = BuildBodyExpression(parameter);

        var expression = lambdaBody is null
            ? BuildDefaultExpression(parameter)
            : Expression.Lambda<Func<T, bool>>(lambdaBody, parameter);

        return expression;
    }

    private Expression? BuildBodyExpression(ParameterExpression parameter)
    {
        Expression? lambdaBody = null;
        foreach (var term in _group.Terms)
        {
            var lambda = term.ToExpression<T>();
            var originalParameter = lambda.Parameters.Single();

            lambdaBody = CombineWithLogicalOperator(lambdaBody, lambda.Body.ReplaceParameter(originalParameter, parameter));
        }

        return lambdaBody;
    }

    private Expression CombineWithLogicalOperator(Expression? left, Expression right)
    {
        if (left is null)
        {
            return right;
        }

        return _group.Operator == LogicalOperator.And
            ? Expression.AndAlso(left, right)
            : Expression.OrElse(left, right);
    }

    private Expression<Func<T, bool>> BuildDefaultExpression(ParameterExpression parameter)
    {
        var defaultValue = _group.Operator == LogicalOperator.And;
        var constExpression = Expression.Constant(defaultValue, typeof(bool));

        return Expression.Lambda<Func<T, bool>>(constExpression, parameter);
    }
}
