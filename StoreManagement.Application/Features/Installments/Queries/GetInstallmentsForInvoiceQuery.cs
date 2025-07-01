namespace StoreManagement.Application.Features.Installments.Queries;

public class GetInstallmentsForInvoiceQuery : IRequest<IEnumerable<InstallmentDto>>
{
    public long InvoiceId { get; set; }
    public InvoiceType InvoiceType { get; set; }
}

public class GetInstallmentsForInvoiceQueryHandler(IDapperRepository dapper)
    : IRequestHandler<GetInstallmentsForInvoiceQuery, IEnumerable<InstallmentDto>>
{
    public async Task<IEnumerable<InstallmentDto>> Handle(GetInstallmentsForInvoiceQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT 
    Id, InstallmentNumber, DueDate, AmountDue, AmountPaid, Status
FROM Installments
WHERE InvoiceId = @InvoiceId AND InvoiceType = @InvoiceType
ORDER BY InstallmentNumber;";

        return await dapper.QueryAsync<InstallmentDto>(sql, new
        {
            request.InvoiceId,
            InvoiceType = (int)request.InvoiceType
        }, cancellationToken: cancellationToken);
    }
}