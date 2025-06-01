namespace StoreManagement.Domain.Specifications;
public interface IExpressionSpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}

