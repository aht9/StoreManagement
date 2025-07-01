namespace StoreManagement.Application.Features.Invoices.Queries;

public class GetInvoicesQuery : IRequest<IEnumerable<InvoiceListDto>>
{
    public InvoiceType InvoiceType { get; set; } // خرید یا فروش
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? PaymentStatus { get; set; } // "Paid" یا "Unpaid"
}

public class GetInvoicesQueryHandler(IDapperRepository dapper)
    : IRequestHandler<GetInvoicesQuery, IEnumerable<InvoiceListDto>>
{
    public async Task<IEnumerable<InvoiceListDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoiceTable = request.InvoiceType == InvoiceType.Sales ? "SalesInvoices" : "PurchaseInvoices";
        var partyJoin = request.InvoiceType == InvoiceType.Sales
            ? "INNER JOIN Customers p ON i.CustomerId = p.Id"
            : "INNER JOIN Stores p ON i.StoreId = p.Id";

        // <<<< کوئری جدید و کاملاً بازنویسی شده >>>>
        var queryBuilder = new StringBuilder($@"
WITH InvoiceStatusCTE AS (
    SELECT 
        i.Id,
        CASE
            -- یک فاکتور اقساطی تنها زمانی پرداخت شده است که تعداد کل اقساط آن بزرگتر از صفر و برابر با تعداد اقساط پرداخت شده آن باشد
            WHEN i.PaymentType = {(int)PaymentType.Installment} THEN
                CASE
                    WHEN (SELECT COUNT(1) FROM Installments WHERE InvoiceId = i.Id AND InvoiceType = @InvoiceType) > 0
                     AND (SELECT COUNT(1) FROM Installments WHERE InvoiceId = i.Id AND InvoiceType = @InvoiceType) = 
                         (SELECT COUNT(1) FROM Installments WHERE InvoiceId = i.Id AND InvoiceType = @InvoiceType AND Status = {(int)InstallmentStatus.Paid})
                    THEN 1
                    ELSE 0
                END
            -- فاکتور نقدی همیشه پرداخت شده فرض می‌شود
            ELSE 1
        END as IsPaid
    FROM {invoiceTable} i
    WHERE i.IsDeleted = 0
)
SELECT 
    i.Id,
    i.InvoiceNumber,
    i.InvoiceDate,
    p.Name AS PartyName,
    i.TotalAmount,
    CASE WHEN cte.IsPaid = 1 THEN N'پرداخت کامل' ELSE N'پرداخت نشده' END AS PaymentStatusText,
    CONVERT(bit, cte.IsPaid) as IsPaid
FROM {invoiceTable} i
{partyJoin}
INNER JOIN InvoiceStatusCTE cte ON i.Id = cte.Id
WHERE 1=1 ");

        var parameters = new Dictionary<string, object>
            {
                { "@InvoiceType", (int)request.InvoiceType }
            };

        if (request.StartDate.HasValue)
        {
            queryBuilder.Append(" AND i.InvoiceDate >= @StartDate");
            parameters.Add("@StartDate", request.StartDate.Value);
        }
        if (request.EndDate.HasValue)
        {
            queryBuilder.Append(" AND i.InvoiceDate <= @EndDate");
            parameters.Add("@EndDate", request.EndDate.Value.AddDays(1).AddTicks(-1)); // برای شامل شدن کل روز پایانی
        }
        if (!string.IsNullOrEmpty(request.PaymentStatus) && request.PaymentStatus != "همه")
        {
            queryBuilder.Append(" AND cte.IsPaid = @IsPaid");
            parameters.Add("@IsPaid", request.PaymentStatus == "پرداخت شده" ? 1 : 0);
        }

        queryBuilder.Append(" ORDER BY i.InvoiceDate DESC;");

        return await dapper.QueryAsync<InvoiceListDto>(queryBuilder.ToString(), new DynamicParameters(parameters), cancellationToken: cancellationToken);
    }
}