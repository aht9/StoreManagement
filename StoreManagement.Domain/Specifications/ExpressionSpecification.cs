namespace StoreManagement.Domain.Specifications;

public abstract class ExpressionSpecification<T> : IExpressionSpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public ExpressionSpecification<T> And(ExpressionSpecification<T> other)
        => new AndExpressionSpecification<T>(this, other);

    public ExpressionSpecification<T> Or(ExpressionSpecification<T> other)
        => new OrExpressionSpecification<T>(this, other);

    public ExpressionSpecification<T> Not()
        => new NotExpressionSpecification<T>(this);

    public static ExpressionSpecification<T> Default()
        => new DefaultExpressionSpecification<T>();

    public static ExpressionSpecification<T> Create(Expression<Func<T, bool>> predicate)
        => new CustomExpressionSpecification<T>(predicate);

    public override string ToString()
        => ToExpression().ToString();
}