namespace StoreManagement.Application.Features.Invoices.Commands;

public class DeleteInvoiceCommand : IRequest
{
    public long InvoiceId { get; set; }
    public InvoiceType InvoiceType { get; set; }
}

public class DeleteInvoiceCommandHandler(
    IGenericRepository<PurchaseInvoice> purchaseInvoiceRepository,
    IGenericRepository<SalesInvoice> salesInvoiceRepository)
    : IRequestHandler<DeleteInvoiceCommand>
{
    public async Task Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {

        if (request.InvoiceType == InvoiceType.Sales)
        {
            var invoice = await salesInvoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null)
                throw new InvalidOperationException("فاکتور فروش مورد نظر یافت نشد.");

            if (invoice.InvoiceStatus == InvoiceStatus.Paid)
                throw new InvalidOperationException("فاکتور پرداخت شده قابل حذف نیست.");

            invoice.MarkAsDeleted();
            await salesInvoiceRepository.UpdateAsync(invoice, cancellationToken);
        }
        else // Purchase
        {
            var invoice = await purchaseInvoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null)
                throw new InvalidOperationException("فاکتور خرید مورد نظر یافت نشد.");

            if (invoice.InvoiceStatus == InvoiceStatus.Paid)
                throw new InvalidOperationException("فاکتور پرداخت شده قابل حذف نیست.");

            invoice.MarkAsDeleted();
            await purchaseInvoiceRepository.UpdateAsync(invoice, cancellationToken);

        }

        await purchaseInvoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}