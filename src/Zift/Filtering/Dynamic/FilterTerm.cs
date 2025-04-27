namespace Zift.Filtering.Dynamic;

public abstract class FilterTerm : IExpressionConvertible
{
    public virtual FilterTerm Negate()
    {
        return new NegatedFilterTerm(this);
    }

    public abstract Expression<Func<T, bool>> ToExpression<T>();

    private class NegatedFilterTerm(FilterTerm term) : FilterTerm
    {
        public override FilterTerm Negate() => term;

        public override Expression<Func<T, bool>> ToExpression<T>()
        {
            return term.ToExpression<T>().Negate();
        }
    }
}
