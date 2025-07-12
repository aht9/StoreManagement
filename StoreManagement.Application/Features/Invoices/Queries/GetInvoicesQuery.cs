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

        // بازنویسی کامل کوئری برای سادگی و کارایی بیشتر
        var queryBuilder = new StringBuilder($@"
SELECT
    i.Id,
    i.InvoiceNumber,
    i.InvoiceDate,
    p.Name AS PartyName,
    i.TotalAmount,
    i.InvoiceStatus,
    CASE i.PaymentType
        WHEN {(int)PaymentType.Cash} THEN N'نقدی'
        WHEN {(int)PaymentType.Installment} THEN N'اقساطی'
        ELSE N'نامشخص'
    END AS PaymentStatusText,
    CASE i.InvoiceStatus
        WHEN {(int)InvoiceStatus.Pending} THEN N'درانتظار پرداخت'
        WHEN {(int)InvoiceStatus.Draft} THEN N'پیش فاکتور'
        WHEN {(int)InvoiceStatus.Cancelled} THEN N'لفو شده'
        WHEN {(int)InvoiceStatus.Paid} THEN N'پرداخت شده'
    END AS InvoiceStatusText
FROM {invoiceTable} i
{partyJoin}
WHERE i.IsDeleted = 0");

        var parameters = new DynamicParameters();

        // افزودن فیلتر بر اساس وضعیت فاکتور (با فرض وجود پراپرتی Status در request)
        if (request.Status.HasValue)
        {
            queryBuilder.Append(" AND i.InvoiceStatus = @Status");
            parameters.Add("@Status", request.Status.Value);
        }

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

        queryBuilder.Append(" ORDER BY i.InvoiceDate DESC;");

        // اجرای کوئری با Dapper
        return await dapper.QueryAsync<InvoiceListDto>(queryBuilder.ToString(), parameters, cancellationToken: cancellationToken);
    }
}