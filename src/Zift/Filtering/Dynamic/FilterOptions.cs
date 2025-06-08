namespace Zift.Filtering.Dynamic;

/// <summary>
/// Configuration options for generating dynamic filter expressions.
/// </summary>
public sealed record FilterOptions
{
    /// <summary>
    /// Enables null guards in the generated expression to safely navigate property paths.
    /// Useful for in-memory LINQ execution.
    /// </summary>
    public bool EnableNullGuards { get; init; } = false;

    /// <summary>
    /// Indicates whether literal values should be converted to constants for parameterization.
    /// Recommended for EF Core queries.
    /// </summary>
    public bool ParameterizeValues { get; init; } = true;
}
