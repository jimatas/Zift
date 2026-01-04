namespace Zift.Querying.ExpressionBuilding;

public sealed class ExpressionBuilderOptions
{
    public bool EnableNullGuards { get; set; } = false;
    public bool ParameterizeValues { get; set; } = true;
}
