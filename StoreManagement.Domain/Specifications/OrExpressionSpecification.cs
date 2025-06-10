namespace StoreManagement.Domain.Specifications;

public class OrExpressionSpecification<T>(ExpressionSpecification<T> left, ExpressionSpecification<T> right)
    : ExpressionSpecification<T>
    where T : BaseEntity
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = left.ToExpression();
        var rightExpr = right.ToExpression();

        var param = Expression.Parameter(typeof(T));
        var combined = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );

        return Expression.Lambda<Func<T, bool>>(combined, param);
    }
}