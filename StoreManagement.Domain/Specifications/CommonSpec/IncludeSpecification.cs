namespace StoreManagement.Domain.Specifications.CommonSpec;

public class IncludeSpecification<T> where T : BaseEntity
{
    private readonly List<Expression<Func<T, object>>> _includes = new();
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    private readonly List<string> _includeStrings = new();
    public IReadOnlyList<string> IncludeStrings => _includeStrings.AsReadOnly();

    public IncludeSpecification<T> Include(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
        return this;
    }

    public IncludeSpecification<T> Include(string includeString)
    {
        _includeStrings.Add(includeString);
        return this;
    }

    public IncludeSpecification<T> Include<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> navigationExpression, Expression<Func<TProperty, object>> thenIncludeExpression)
        where TProperty : class
    {
        var propertyName = GetNavigationPropertyName(navigationExpression);
        var thenPropertyName = GetThenIncludePropertyName(thenIncludeExpression);
        _includeStrings.Add($"{propertyName}.{thenPropertyName}");
        return this;
    }


    /// <summary>
    /// Extracts the name of the navigation property from the provided expression.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being navigated to.</typeparam>
    /// <param name="expression">An expression representing the navigation property.</param>
    /// <returns>The name of the navigation property.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided expression is not a member access expression.
    /// </exception>
    private string GetNavigationPropertyName<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        throw new ArgumentException("Expression is not a member access expression.", nameof(expression));
    }

    /// <summary>
    /// Extracts the name of the property to be included in a "ThenInclude" operation from the provided expression.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being navigated to.</typeparam>
    /// <param name="expression">An expression representing the property to be included.</param>
    /// <returns>The name of the property to be included.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided expression is not a member access expression or a valid unary conversion.
    /// </exception>
    private string GetThenIncludePropertyName<TProperty>(Expression<Func<TProperty, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        // Handle case when the expression is a conversion (e.g., x => (object)x.Property)
        else if (expression.Body is UnaryExpression unaryExpression &&
                 unaryExpression.Operand is MemberExpression operandMemberExpression)
        {
            return operandMemberExpression.Member.Name;
        }
        throw new ArgumentException("Expression is not a member access expression.", nameof(expression));
    }

}