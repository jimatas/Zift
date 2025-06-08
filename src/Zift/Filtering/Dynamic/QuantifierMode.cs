namespace Zift.Filtering.Dynamic;

/// <summary>
/// Specifies how a condition should be applied to elements in a collection.
/// </summary>
public enum QuantifierMode
{
    /// <summary>
    /// At least one element must satisfy the condition.
    /// </summary>
    Any = 1,

    /// <summary>
    /// All elements must satisfy the condition.
    /// </summary>
    All
}
