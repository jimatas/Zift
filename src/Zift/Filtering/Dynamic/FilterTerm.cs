namespace Zift.Filtering.Dynamic;

/// <summary>
/// Represents a composable filter term that can be converted into a predicate expression.
/// </summary>
public abstract class FilterTerm
{
    /// <summary>
    /// Negates this filter term.
    /// </summary>
    /// <returns>A new filter term representing the negation of this term.</returns>
    public virtual FilterTerm Negate() => new NegatedFilterTerm(this);

    /// <summary>
    /// Converts this filter term into a LINQ expression.
    /// </summary>
    /// <typeparam name="T">The type of the elements to filter.</typeparam>
    /// <param name="options">Optional filter options.</param>
    /// <returns>A predicate expression for filtering elements.</returns>
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
