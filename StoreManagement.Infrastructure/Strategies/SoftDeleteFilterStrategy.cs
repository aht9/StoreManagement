namespace StoreManagement.Infrastructure.Strategies;

public class SoftDeleteFilterStrategy<T> : IFilterStrategy<T> where T : BaseEntity
{
    private readonly SoftDeleteSpecification<T> _notDeletedSpec;

    public SoftDeleteFilterStrategy()
    {
        var hasIsDeleted = typeof(T).GetProperty("IsDeleted") != null;
        if (hasIsDeleted)
        {
            _notDeletedSpec = new SoftDeleteSpecification<T>(false);
        }
    }

    public IExpressionSpecification<T> ApplyFilter(IExpressionSpecification<T> specification)
    {
        if (_notDeletedSpec != null)
        {
            return specification.AndSpecification(_notDeletedSpec);
        }

        return specification;
    }
}