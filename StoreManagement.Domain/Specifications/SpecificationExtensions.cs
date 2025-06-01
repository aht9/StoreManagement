namespace StoreManagement.Domain.Specifications;

public static class SpecificationExtensions
{
    public static ExpressionSpecification<T> AndSpecification<T>(
        this IExpressionSpecification<T> spec,
        ExpressionSpecification<T> other)
    {
        // اگر spec از نوع ExpressionSpecification<T> باشد، از متد And آن استفاده می‌کنیم
        if (spec is ExpressionSpecification<T> expressionSpec)
        {
            return expressionSpec.And(other);
        }

        // در غیر این صورت، یک AndExpressionSpecification جدید می‌سازیم
        return new AndExpressionSpecification<T>(
            new ExpressionSpecificationAdapter<T>(spec),
            other
        );
    }
}

// کلاس کمکی برای تبدیل IExpressionSpecification به ExpressionSpecification
public class ExpressionSpecificationAdapter<T> : ExpressionSpecification<T>
{
    private readonly IExpressionSpecification<T> _specification;

    public ExpressionSpecificationAdapter(IExpressionSpecification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        return _specification.ToExpression();
    }
}