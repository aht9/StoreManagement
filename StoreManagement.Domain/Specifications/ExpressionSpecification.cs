namespace StoreManagement.Domain.Specifications;

public abstract class ExpressionSpecification<T> : IExpressionSpecification<T> where T : BaseEntity
{
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();

    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public ExpressionSpecification<T> And(ExpressionSpecification<T> other)
    {
        // The problematic 'if' is removed. This now works for any ExpressionSpecification.
        var combinedSpec = new AndExpressionSpecification<T>(this, other);
        combinedSpec.AddIncludesFrom(this);
        combinedSpec.AddIncludesFrom(other);
        return combinedSpec;
    }
    public ExpressionSpecification<T> Or(ExpressionSpecification<T> other)
    {
        var combinedSpec = new OrExpressionSpecification<T>(this, other);
        combinedSpec.AddIncludesFrom(this);
        combinedSpec.AddIncludesFrom(other);
        return combinedSpec;
    }
    public ExpressionSpecification<T> Not()
    {
        var notSpec = new NotExpressionSpecification<T>(this);
        notSpec.AddIncludesFrom(this);
        return notSpec;
    }

    protected void AddIncludesFrom(ExpressionSpecification<T> spec)
    {
        Includes.AddRange(spec.Includes);
        IncludeStrings.AddRange(spec.IncludeStrings);
    }


    // Static helpers remain unchanged
    public static ExpressionSpecification<T> Default() => new DefaultExpressionSpecification<T>();
    public static ExpressionSpecification<T> Create(Expression<Func<T, bool>> predicate) => new CustomExpressionSpecification<T>(predicate);
    public override string ToString() => ToExpression().ToString();
}