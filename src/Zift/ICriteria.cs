namespace Zift;

public interface ICriteria<T>
{
    IQueryable<T> ApplyTo(IQueryable<T> query);
}
