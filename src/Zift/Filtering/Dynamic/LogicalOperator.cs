namespace Zift.Filtering.Dynamic;

/// <summary>
/// Specifies how multiple filter terms are combined in a <see cref="FilterGroup"/>.
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// Combines terms using logical AND.
    /// </summary>
    And = 1,

    /// <summary>
    /// Combines terms using logical OR.
    /// </summary>
    Or
}
