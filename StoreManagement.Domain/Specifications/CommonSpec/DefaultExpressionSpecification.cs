namespace StoreManagement.Domain.Specifications.CommonSpec;

public class DefaultExpressionSpecification<T> : ExpressionSpecification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
        => x => true;
}