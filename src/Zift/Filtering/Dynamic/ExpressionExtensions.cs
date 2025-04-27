namespace Zift.Filtering.Dynamic;

internal static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> Negate<T>(this Expression<Func<T, bool>> expression)
    {
        return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
    }

    public static Expression ReplaceParameter(
        this Expression expression,
        ParameterExpression target,
        ParameterExpression replacement)
    {
        return new ParameterReplacer(target, replacement).Visit(expression);
    }

    private class ParameterReplacer(ParameterExpression target, ParameterExpression replacement)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            return parameter == target ? replacement : base.VisitParameter(parameter);
        }
    }
}
