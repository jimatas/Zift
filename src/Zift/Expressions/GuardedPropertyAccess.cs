namespace Zift.Expressions;

internal sealed record GuardedPropertyAccess(
    Expression Value,
    Expression? NullGuard);
