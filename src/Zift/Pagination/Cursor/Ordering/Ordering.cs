namespace Zift.Pagination.Cursor.Ordering;

internal sealed class Ordering<T>
{
    public static readonly Ordering<T> Empty = new(Array.Empty<OrderingClause<T>>());

    private Ordering(IReadOnlyList<OrderingClause<T>> clauses) => Clauses = clauses;

    public IReadOnlyList<OrderingClause<T>> Clauses { get; }
    public bool IsEmpty => Clauses.Count == 0;

    public Ordering<T> Append(OrderingClause<T> clause)
    {
        if (IsEmpty)
        {
            return new Ordering<T>([clause]);
        }

        var clauses = new OrderingClause<T>[Clauses.Count + 1];
        for (var i = 0; i < Clauses.Count; i++)
        {
            clauses[i] = Clauses[i];
        }

        clauses[^1] = clause;

        return new Ordering<T>(clauses);
    }

    public IOrderedQueryable<T> ApplyTo(IQueryable<T> query)
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException(
                "Cannot apply ordering without any ordering clauses.");
        }

        var orderedQuery = Clauses[0].ApplyTo(query);

        for (var i = 1; i < Clauses.Count; i++)
        {
            orderedQuery = Clauses[i].ApplyTo(orderedQuery);
        }

        return orderedQuery;
    }

    public Ordering<T> Reverse()
    {
        if (IsEmpty)
        {
            return this;
        }

        var reversedClauses = Clauses
            .Select(clause => clause.Reverse())
            .ToArray();

        return new Ordering<T>(reversedClauses);
    }

    public static Ordering<T> Parse(
        string orderByClause,
        OrderingOptions options)
    {
        if (string.IsNullOrWhiteSpace(orderByClause))
        {
            return Empty;
        }

        var clauses = new OrderingParser<T>(options).Parse(orderByClause);

        return new Ordering<T>(clauses);
    }
}
