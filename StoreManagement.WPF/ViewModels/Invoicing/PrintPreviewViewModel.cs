using System.Diagnostics;

namespace StoreManagement.WPF.ViewModels.Invoicing;
public partial class PrintPreviewViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly long _invoiceId;
    private readonly InvoiceType _invoiceType;

    [ObservableProperty]
    private PrintInvoiceDto _invoice;

    public PrintPreviewViewModel(IMediator mediator, long invoiceId, InvoiceType invoiceType)
    {
        _mediator = mediator;
        _invoiceId = invoiceId;
        _invoiceType = invoiceType;
    }

    /// <summary>
    /// این متد وظیفه بارگذاری ناهمزمان داده‌های فاکتور را بر عهده دارد.
    /// </summary>
    public async Task InitializeAsync()
    {
        Invoice = await _mediator.Send(new GetInvoiceForPrintQuery { InvoiceId = _invoiceId, InvoiceType = _invoiceType });
    }
    
    [RelayCommand]
    private async Task Print()
    {
        var pdfBytes = await _mediator.Send(new GenerateInvoicePdfQuery { InvoiceData = this.Invoice });
        
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Invoice.InvoiceNumber}.pdf");
        await File.WriteAllBytesAsync(tempPath, pdfBytes);
        
        new Process { StartInfo = new ProcessStartInfo(tempPath) { UseShellExecute = true } }.Start();
    }
}