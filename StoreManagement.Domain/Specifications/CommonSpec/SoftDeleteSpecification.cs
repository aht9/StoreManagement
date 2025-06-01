namespace StoreManagement.Domain.Specifications.CommonSpec;

public class SoftDeleteSpecification<T> : ExpressionSpecification<T> where T : BaseEntity
{
    private readonly bool _includeDeleted;
    public SoftDeleteSpecification(bool includeDeleted = false)
    {
        _includeDeleted = includeDeleted;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        // Check if the entity has IsDeleted property
        var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
        if (isDeletedProperty == null || isDeletedProperty.PropertyType != typeof(bool))
            return x => true;
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, isDeletedProperty);
        if (_includeDeleted)
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), parameter);
        else
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(property, Expression.Constant(false)), parameter);
    }
}