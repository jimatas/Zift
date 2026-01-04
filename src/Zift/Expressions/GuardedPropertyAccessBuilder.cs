namespace Zift.Expressions;

using Types;

internal static class GuardedPropertyAccessBuilder
{
    public static GuardedPropertyAccess Build(
        Expression root,
        IReadOnlyList<string> propertyPath,
        bool enableNullGuards = true,
        bool guardRoot = false)
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
        Expression guardedExpression,
        bool enableNullGuards)
    {
        if (!enableNullGuards || !guardedExpression.Type.IsNullable())
        {
            return existingGuard;
        }

        var isNotNull = Expression.NotEqual(
            guardedExpression,
            Expression.Constant(null, guardedExpression.Type));

        return existingGuard is null
            ? isNotNull
            : Expression.AndAlso(existingGuard, isNotNull);
    }
}
