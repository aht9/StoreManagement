namespace StoreManagement.Domain.Specifications.CommonSpec;

public class DefaultExpressionSpecification<T> : ExpressionSpecification<T> where T : BaseEntity
{
    public override Expression<Func<T, bool>> ToExpression()
        => x => true;
}