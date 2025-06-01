namespace StoreManagement.Domain.Specifications.CommonSpec;

public sealed class OrderBySpecification<T, TKey>(Expression<Func<T, TKey>> orderByExpression, bool descending = false)
    where T : BaseEntity
{
    public Expression<Func<T, TKey>> OrderByExpression => orderByExpression;
    public bool Descending => descending;
}