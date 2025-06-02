namespace StoreManagement.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(ApplicationDbContext dbContext)
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ApplicationDbContext _dbContext = dbContext;


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.ToString().EndsWith("Query"))
        {
            return await next();
        }

        var response = default(TResponse);
        var typeName = request.GetType();
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (_dbContext.HasActiveTransaction)
                {
                    return await next();
                }

                var strategy = _dbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    Guid transactionId;

                    using (var transaction = await _dbContext.BeginTransactionAsync())
                    {
                        response = await next();

                        await _dbContext.CommitTransactionAsync(transaction);
                        transactionId = transaction.TransactionId;
                    }
                });

                return response;
            }
            catch (Exception ex)
            {
                if (i >= 3)
                    throw;
            }
        }

        return response;
    }
}