namespace StoreManagement.Application.Features.Invoices.Queries;

public class GetInvoicesQuery : IRequest<IEnumerable<InvoiceListDto>>
{
    public InvoiceType InvoiceType { get; set; } // Purchase یا Sales
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public InvoiceStatus? Status { get; set; }
}

public class GetInvoicesQueryHandler(IDapperRepository dapper)
    : IRequestHandler<GetInvoicesQuery, IEnumerable<InvoiceListDto>>
{
    public async Task<IEnumerable<InvoiceListDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        // تعیین نام جدول و جوین مربوطه بر اساس نوع فاکتور
        var invoiceTable = request.InvoiceType == InvoiceType.Sales ? "SalesInvoices" : "PurchaseInvoices";
        var partyJoin = request.InvoiceType == InvoiceType.Sales
            ? "INNER JOIN Customers p ON i.CustomerId = p.Id"
            : "INNER JOIN Stores p ON i.StoreId = p.Id";

        var queryBuilder = new StringBuilder($@"
-- CTE برای محاسبه وضعیت پرداخت واقعی هر فاکتور
WITH InvoicePaymentStatusCTE AS (
    SELECT 
        i.Id,
        CASE
            -- اگر نقدی باشد، همیشه پرداخت شده است
            WHEN i.PaymentType = {(int)PaymentType.Cash} THEN 1
            -- اگر اقساطی باشد، تنها زمانی پرداخت شده است که تعداد کل اقساط با تعداد اقساط پرداخت شده برابر باشد
            WHEN i.PaymentType = {(int)PaymentType.Installment} THEN 
                CASE
                    WHEN (SELECT COUNT(1) FROM Installments WHERE InvoiceId = i.Id AND InvoiceType = @InvoiceType) > 0 AND
                         (SELECT COUNT(1) FROM Installments WHERE InvoiceId = i.Id AND InvoiceType = @InvoiceType) = 
                         (SELECT COUNT(1) FROM Installments WHERE InvoiceId = i.Id AND InvoiceType = @InvoiceType AND Status = {(int)InstallmentStatus.Paid})
                    THEN 1
                    ELSE 0
                END
            ELSE 0
        END AS IsFullyPaid
    FROM {invoiceTable} i
    WHERE i.IsDeleted = 0
)
-- کوئری اصلی برای واکشی اطلاعات
SELECT 
    i.Id,
    i.InvoiceNumber,
    i.InvoiceDate,
    p.Name AS PartyName,
    i.TotalAmount,
    i.InvoiceStatus,
    CASE WHEN cte.IsFullyPaid = 1 THEN N'پرداخت کامل' ELSE N'پرداخت نشده' END AS PaymentStatusText,
    CONVERT(bit, cte.IsFullyPaid) as IsPaid
FROM {invoiceTable} i
{partyJoin}
INNER JOIN InvoicePaymentStatusCTE cte ON i.Id = cte.Id
WHERE 1=1 ");

        var parameters = new DynamicParameters();
        parameters.Add("@InvoiceType", (int)request.InvoiceType);


        //// افزودن فیلتر بر اساس وضعیت فاکتور (با فرض وجود پراپرتی Status در request)
        //if (request.Status.HasValue)
        //{
        //    queryBuilder.Append(" AND i.InvoiceStatus = @Status");
        //    parameters.Add("@Status", request.Status.Value);
        //}

        // افزودن فیلترهای تاریخ (بدون تغییر)
        if (request.StartDate.HasValue)
        {
            queryBuilder.Append(" AND i.InvoiceDate >= @StartDate");
            parameters.Add("@StartDate", request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            queryBuilder.Append(" AND i.InvoiceDate <= @EndDate");
            parameters.Add("@EndDate", request.EndDate.Value.AddDays(1).AddTicks(-1));
        }

        if (request.Status.HasValue)
        {
            bool isPaidFilter = request.Status.Value == InvoiceStatus.Paid;
            queryBuilder.Append(" AND cte.IsFullyPaid = @IsPaid");
            parameters.Add("@IsPaid", isPaidFilter);
        }

        queryBuilder.Append(" ORDER BY i.InvoiceDate DESC;");

        // اجرای کوئری با Dapper
        return await dapper.QueryAsync<InvoiceListDto>(queryBuilder.ToString(), parameters, cancellationToken: cancellationToken);
    }
}