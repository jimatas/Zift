namespace Zift.Sorting;

public interface ISortCriterion
{
    string Property { get; }
    SortDirection Direction { get; }
}
