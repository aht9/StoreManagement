namespace StoreManagement.Domain.Specifications.CommonSpec;

public class IncludeSpecification<T> : ExpressionSpecification<T> where T : BaseEntity
{
    /// <summary>
    /// This specification does not filter, so its expression is always true.
    /// </summary>
    public override Expression<Func<T, bool>> ToExpression()
    {
        return entity => true;
    }

    /// <summary>
    /// Adds an include expression. The 'new' keyword is used to hide the base method
    /// and return the more specific IncludeSpecification type for a fluent API.
    /// </summary>
    public new IncludeSpecification<T> Include(Expression<Func<T, object>> includeExpression)
    {
        base.Includes.Add(includeExpression);
        return this;
    }

    /// <summary>
    /// Adds an include expression by string.
    /// </summary>
    public new IncludeSpecification<T> Include(string includeString)
    {
        base.IncludeStrings.Add(includeString);
        return this;
    }

    /// <summary>
    /// Adds a ThenInclude expression using strings.
    /// </summary>
    public IncludeSpecification<T> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationExpression, Expression<Func<TProperty, object>> thenIncludeExpression)
        where TProperty : class
    {
        var propertyName = GetNavigationPropertyName(navigationExpression);
        var thenPropertyName = GetThenIncludePropertyName(thenIncludeExpression);
        base.IncludeStrings.Add($"{propertyName}.{thenPropertyName}");
        return this;
    }

    // Private helpers from your original code
    private string GetNavigationPropertyName<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        throw new ArgumentException("Expression is not a member access expression.", nameof(expression));
    }

    private string GetThenIncludePropertyName<TProperty>(Expression<Func<TProperty, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (expression.Body is UnaryExpression unaryExpression &&
                 unaryExpression.Operand is MemberExpression operandMemberExpression)
        {
            return operandMemberExpression.Member.Name;
        }
        throw new ArgumentException("Expression is not a member access expression.", nameof(expression));
    }
}