namespace StoreManagement.Domain.Specifications;

public class OrExpressionSpecification<T> : ExpressionSpecification<T>
{
    private readonly ExpressionSpecification<T> _left;
    private readonly ExpressionSpecification<T> _right;

    public OrExpressionSpecification(ExpressionSpecification<T> left, ExpressionSpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();

        var param = Expression.Parameter(typeof(T));
        var combined = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );

        return Expression.Lambda<Func<T, bool>>(combined, param);
    }
}