namespace Zift.Sorting;

/// <summary>
/// Base implementation of <see cref="ISortCriterion"/>.
/// </summary>
/// <param name="property">The name of the property to sort by.</param>
/// <param name="direction">The sort direction.</param>
public abstract class SortCriterion(string property, SortDirection direction) : ISortCriterion
{
    /// <inheritdoc/>
    public string Property { get; } = property.ThrowIfNullOrEmpty();

    /// <inheritdoc/>
    public SortDirection Direction { get; } = direction;
}
