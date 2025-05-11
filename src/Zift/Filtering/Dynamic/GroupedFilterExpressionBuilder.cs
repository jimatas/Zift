namespace Zift.Filtering.Dynamic;

internal class GroupedFilterExpressionBuilder<T>(FilterGroup group, FilterOptions? options)
{
    private readonly FilterGroup _group = group;
    private readonly FilterOptions? _options = options;

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
            var lambda = term.ToExpression<T>(_options);
            var originalParameter = lambda.Parameters.Single();

            lambdaBody = CombineWithLogicalOperator(lambdaBody, lambda.Body.ReplaceParameter(originalParameter, parameter));
        }

        return lambdaBody;
    }

    private Expression CombineWithLogicalOperator(Expression? leftOperand, Expression rightOperand)
    {
        if (leftOperand is null)
        {
            return rightOperand;
        }

        return _group.Operator == LogicalOperator.And
            ? Expression.AndAlso(leftOperand, rightOperand)
            : Expression.OrElse(leftOperand, rightOperand);
    }

    private Expression<Func<T, bool>> BuildDefaultExpression(ParameterExpression parameter)
    {
        var defaultValue = _group.Operator == LogicalOperator.And;
        var constExpression = Expression.Constant(defaultValue, typeof(bool));

        return Expression.Lambda<Func<T, bool>>(constExpression, parameter);
    }
}
