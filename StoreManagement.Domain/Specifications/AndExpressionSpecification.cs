namespace StoreManagement.Domain.Specifications;

public class AndExpressionSpecification<T> : ExpressionSpecification<T>
{
    private readonly ExpressionSpecification<T> _left;
    private readonly ExpressionSpecification<T> _right;

    public AndExpressionSpecification(ExpressionSpecification<T> left, ExpressionSpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();

        var param = Expression.Parameter(typeof(T));
        var combined = Expression.AndAlso(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );

        return Expression.Lambda<Func<T, bool>>(combined, param);
    }
}