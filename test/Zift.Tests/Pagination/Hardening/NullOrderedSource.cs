using System.Collections;
using System.Linq.Expressions;
using Zift.Pagination.Cursor;

namespace Zift.Pagination.Tests;

internal static class NullOrderedSource
{
    public static IQueryable<T> Create<T>(IEnumerable<T> items, NullOrdering nullOrdering)
    {
        var inner = items.ToList().AsQueryable();

        return nullOrdering == NullOrdering.NullsCompareHigher
            ? new NullsHigherQuery<T>(new NullsHigherProvider(inner))
            : inner;
    }

    public static IComparer<TKey> Comparer<TKey>(NullOrdering nullOrdering)
    {
        return nullOrdering == NullOrdering.NullsCompareHigher
            ? new NullsHigherComparer<TKey>()
            : System.Collections.Generic.Comparer<TKey>.Default;
    }

    private sealed class NullsHigherComparer<TKey> : IComparer<TKey>
    {
        public int Compare(TKey? x, TKey? y)
        {
            return (x is null, y is null) switch
            {
                (true, true) => 0,
                (true, false) => 1,
                (false, true) => -1,
                _ => System.Collections.Generic.Comparer<TKey>.Default.Compare(x, y)
            };
        }
    }

    private sealed class NullsHigherProvider(IQueryable root) : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments()[0];
            var queryType = typeof(NullsHigherQuery<>).MakeGenericType(elementType);

            return (IQueryable)Activator.CreateInstance(queryType, this, expression)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new NullsHigherQuery<TElement>(this, expression);
        }

        public object? Execute(Expression expression)
        {
            return root.Provider.Execute(Rewrite(expression));
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return root.Provider.Execute<TResult>(Rewrite(expression));
        }

        public IEnumerator<TElement> Enumerate<TElement>(Expression expression)
        {
            return root.Provider.CreateQuery<TElement>(Rewrite(expression)).GetEnumerator();
        }

        private Expression Rewrite(Expression expression)
        {
            return new Rewriter(this, root.Expression).Visit(expression);
        }
    }

    private sealed class NullsHigherQuery<T> : IOrderedQueryable<T>
    {
        private readonly NullsHigherProvider _provider;

        public NullsHigherQuery(NullsHigherProvider provider, Expression? expression = null)
        {
            _provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator() => _provider.Enumerate<T>(Expression);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class Rewriter(NullsHigherProvider provider, Expression rootExpression) : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node.Value is IQueryable query && ReferenceEquals(query.Provider, provider)
                ? rootExpression
                : base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visited = (MethodCallExpression)base.VisitMethodCall(node);
            var method = visited.Method;

            if (method.DeclaringType != typeof(Queryable) ||
                method.Name is not (nameof(Queryable.OrderBy) or nameof(Queryable.OrderByDescending) or
                    nameof(Queryable.ThenBy) or nameof(Queryable.ThenByDescending)) ||
                method.GetParameters().Length != 2)
            {
                return visited;
            }

            var typeArguments = method.GetGenericArguments();
            var comparer = Activator.CreateInstance(typeof(NullsHigherComparer<>).MakeGenericType(typeArguments[1]))!;
            var withComparer = typeof(Queryable).GetMethods()
                .Single(m => m.Name == method.Name && m.GetParameters().Length == 3)
                .MakeGenericMethod(typeArguments);

            return Expression.Call(
                withComparer,
                visited.Arguments[0],
                visited.Arguments[1],
                Expression.Constant(comparer, typeof(IComparer<>).MakeGenericType(typeArguments[1])));
        }
    }
}
