namespace Zift.Sorting;

public interface ISortCriteria<T> : ICriteria<T>, IEnumerable<ISortCriterion<T>>;
