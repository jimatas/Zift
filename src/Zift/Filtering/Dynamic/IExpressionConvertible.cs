namespace Zift.Filtering.Dynamic;

public interface IExpressionConvertible
{
    Expression<Func<T, bool>> ToExpression<T>();
}
