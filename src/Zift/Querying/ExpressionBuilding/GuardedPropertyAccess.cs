namespace Zift.Querying.ExpressionBuilding;

internal sealed record GuardedPropertyAccess(
    Expression Value,
    Expression? NullGuard);
