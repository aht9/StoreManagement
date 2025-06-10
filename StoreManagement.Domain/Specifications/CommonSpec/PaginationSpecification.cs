namespace StoreManagement.Domain.Specifications.CommonSpec;

public sealed class PaginationSpecification<T>(int pageNumber, int pageSize) : ExpressionSpecification<T>
    where T : BaseEntity
{
    private readonly int _skip = (pageNumber - 1) * pageSize;

    public int Skip => _skip;
    public int Take => pageSize;

    public override Expression<Func<T, bool>> ToExpression()
    {
        return entity => true;
    }
}