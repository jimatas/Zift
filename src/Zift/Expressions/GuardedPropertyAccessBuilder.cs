namespace Zift.Expressions;

using Types;

internal static class GuardedPropertyAccessBuilder
{
    public static GuardedPropertyAccess Build(
        Expression root,
        IReadOnlyList<string> propertyPath,
        bool guardRoot = false,
        bool enableNullGuards = true)
    {
        Expression current = root;
        Expression? nullGuard = null;

        if (guardRoot)
        {
            nullGuard = CombineNullGuard(nullGuard, current, enableNullGuards);
        }

        foreach (var propertyName in propertyPath)
        {
            current = Expression.Property(current, propertyName);
            nullGuard = CombineNullGuard(nullGuard, current, enableNullGuards);
        }

        return new GuardedPropertyAccess(current, nullGuard);
    }

    private static Expression? CombineNullGuard(
        Expression? existingGuard,
        Expression expression,
        bool enableNullGuards)
    {
        if (!enableNullGuards || !expression.Type.IsNullable())
        {
            return existingGuard;
        }

        var notNull = Expression.NotEqual(
            expression,
            Expression.Constant(null, expression.Type));

        return existingGuard is null
            ? notNull
            : Expression.AndAlso(existingGuard, notNull);
    }
}
