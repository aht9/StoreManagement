namespace StoreManagement.Infrastructure.Strategies;

public interface IFilterStrategy<T>
{
    IExpressionSpecification<T> ApplyFilter(IExpressionSpecification<T> specification);
}