namespace Zift.Filtering.Dynamic;

internal static class ExpressionExtensions
{
    /// <summary>
    /// Negates a boolean lambda expression.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter.</typeparam>
    /// <param name="expression">The expression to negate.</param>
    /// <returns>A new expression representing the logical negation of the original.</returns>
    public static Expression<Func<T, bool>> Negate<T>(this Expression<Func<T, bool>> expression) =>
        Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);

    /// <summary>
    /// Replaces a parameter in an expression with another parameter.
    /// </summary>
    /// <param name="expression">The source expression.</param>
    /// <param name="target">The parameter to replace.</param>
    /// <param name="replacement">The parameter to use as a replacement.</param>
    /// <returns>A new expression with the parameter replaced.</returns>
    public static Expression ReplaceParameter(
        this Expression expression,
        ParameterExpression target,
        ParameterExpression replacement) => new ParameterReplacer(target, replacement).Visit(expression);

    private sealed class ParameterReplacer(ParameterExpression target, ParameterExpression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression parameter) =>
            parameter == target
                ? replacement
                : base.VisitParameter(parameter);
    }
}
