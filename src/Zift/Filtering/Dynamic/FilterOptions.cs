namespace Zift.Filtering.Dynamic;

public sealed record FilterOptions
{
    public bool EnableNullGuards { get; init; } = true;
    public bool ParameterizeValues { get; init; } = true;
}
