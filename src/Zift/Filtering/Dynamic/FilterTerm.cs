namespace Zift.Filtering.Dynamic;

public abstract class FilterTerm
{
    public virtual FilterTerm Negate() => new NegatedFilterTerm(this);

    public abstract Expression<Func<T, bool>> ToExpression<T>(FilterOptions? options = null);

    private class NegatedFilterTerm(FilterTerm term) : FilterTerm
    {
        public override FilterTerm Negate() => term;

        public override Expression<Func<T, bool>> ToExpression<T>(FilterOptions? options = null)
        {
            return term.ToExpression<T>(options).Negate();
        }
    }
}
