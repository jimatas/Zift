namespace Zift.Sorting.Dynamic;

public interface ISortDirectiveParser<T>
{
    IEnumerable<ISortCriterion<T>> Parse(string directives);
}
