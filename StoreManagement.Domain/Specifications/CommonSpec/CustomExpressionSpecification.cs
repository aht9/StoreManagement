namespace StoreManagement.Domain.Specifications.CommonSpec;

public class CustomExpressionSpecification<T>(Expression<Func<T, bool>> predicate) : ExpressionSpecification<T> where T : BaseEntity
{
    public override Expression<Func<T, bool>> ToExpression()
        => predicate;
}