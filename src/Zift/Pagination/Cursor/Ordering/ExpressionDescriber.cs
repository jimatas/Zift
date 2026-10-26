namespace Zift.Pagination.Cursor.Ordering;

internal static class ExpressionDescriber
{
    internal static string Describe(this Expression expression)
    {
        var inlined = new CapturedValueInliningVisitor().Visit(expression);

        return new InvariantConstantFormattingVisitor().Visit(inlined).ToString();
    }

    private sealed class CapturedValueInliningVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var owner = Visit(node.Expression);

            if (owner is ConstantExpression { Value: { } instance })
            {
                switch (node.Member)
                {
                    case FieldInfo field:
                        return Expression.Constant(field.GetValue(instance), node.Type);

                    case PropertyInfo { CanRead: true } property when property.GetIndexParameters().Length == 0:
                        return Expression.Constant(property.GetValue(instance), node.Type);
                }
            }

            return node.Update(owner);
        }
    }

    private sealed class InvariantConstantFormattingVisitor : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node.Value is IFormattable value
                ? Expression.Parameter(node.Type, Format(value))
                : node;
        }

        private static string Format(IFormattable value)
        {
            return value switch
            {
                DateTime or DateTimeOffset or DateOnly or TimeOnly =>
                    value.ToString("O", CultureInfo.InvariantCulture),

                _ => value.ToString(null, CultureInfo.InvariantCulture)
            };
        }
    }
}
