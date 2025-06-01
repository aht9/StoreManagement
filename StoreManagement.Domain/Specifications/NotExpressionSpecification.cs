namespace StoreManagement.Domain.Specifications;

public class NotExpressionSpecification<T> : ExpressionSpecification<T>
{
    private readonly ExpressionSpecification<T> _specification;

    public NotExpressionSpecification(ExpressionSpecification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var param = Expression.Parameter(typeof(T));
        var notExpression = Expression.Not(Expression.Invoke(expression, param));
        return Expression.Lambda<Func<T, bool>>(notExpression, param);
    }
}