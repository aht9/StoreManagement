namespace StoreManagement.Domain.Specifications.CommonSpec;

public class CustomExpressionSpecification<T>(Expression<Func<T, bool>> predicate) : ExpressionSpecification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
        => predicate;
}