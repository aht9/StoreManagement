namespace StoreManagement.Application.Features.Customers.Command;

public class DeleteCustomerCommand : IRequest<Result>
{
    public long Id { get; set; }
}

public class DeleteCustomerCommandHandler(
    IGenericRepository<Customer> customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCustomerCommandHandler> logger)
    : IRequestHandler<DeleteCustomerCommand, Result>
{
    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var includeSpec = new IncludeSpecification<Customer>().Include(c => c.SalesInvoices);
            var customerSpec = new CustomExpressionSpecification<Customer>(c => c.Id == request.Id);
            var finalSpec = customerSpec.And(includeSpec);

            var customer = await customerRepository.FirstOrDefaultAsync(finalSpec, cancellationToken);
            if (customer == null || customer.IsDeleted)
            {
                return Result.Failure("مشتری مورد نظر یافت نشد.");
            }

            if (customer.SalesInvoices.Any(i => i.InvoiceStatus != InvoiceStatus.Paid))
            {
                return Result.Failure("امکان حذف مشتری به دلیل داشتن فاکتورهای پرداخت نشده وجود ندارد.");
            }

            customer.MarkAsDeleted();

            await customerRepository.UpdateAsync(customer, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Customer with ID {CustomerId} deleted successfully.", customer.Id);
            return Result.Success();
        }
        catch (ArgumentException argEx)
        {
            logger.LogWarning("Business rule validation failed while deleting customer {CustomerId}: {ErrorMessage}", request.Id, argEx.Message);
            return Result.Failure(argEx.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting customer with ID {CustomerId}.", request.Id);
            return Result.Failure("خطا در حذف مشتری. لطفاً دوباره تلاش کنید.");
        }
    }
}