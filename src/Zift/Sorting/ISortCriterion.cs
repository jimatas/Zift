namespace Zift.Sorting;

/// <summary>
/// Represents a single sort criterion based on a property and direction.
/// </summary>
public interface ISortCriterion
{
    /// <summary>
    /// The name of the property to sort by.
    /// </summary>
    string Property { get; }

    /// <summary>
    /// The direction of the sort.
    /// </summary>
    SortDirection Direction { get; }
}
