namespace Zift.Pagination.Cursor.Execution;

internal static class ParameterReplacer
{
    internal static Expression ReplaceParameter(
        this Expression expression,
        ParameterExpression original,
        ParameterExpression replacement)
    {
        return new ParameterReplacingVisitor(original, replacement).Visit(expression);
    }

    private sealed class ParameterReplacingVisitor(
        ParameterExpression original,
        ParameterExpression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            return parameter == original
                ? replacement
                : base.VisitParameter(parameter);
        }
    }
}
