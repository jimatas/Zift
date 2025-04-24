namespace Zift.Sorting;

public abstract class SortCriterion(string property, SortDirection direction) : ISortCriterion
{
    public string Property { get; } = property.ThrowIfNullOrEmpty();
    public SortDirection Direction { get; } = direction;
}
