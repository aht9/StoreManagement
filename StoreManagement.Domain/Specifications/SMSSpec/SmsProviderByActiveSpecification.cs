namespace StoreManagement.Domain.Specifications.SMSSpec;

public class SmsProviderByActiveSpecification : IExpressionSpecification<SmsProvider>
{
    public Expression<Func<SmsProvider, bool>> ToExpression()
    {
        return p => p.IsActive;
    }
}