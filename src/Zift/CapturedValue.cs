namespace Zift;

internal static class CapturedValue
{
    public static UnaryExpression Create(object value, Type type)
    {
        Expression<Func<object>> accessor = () => value;

        return Expression.Convert(accessor.Body, type);
    }
}
